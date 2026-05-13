using ArtificialBuilder_EDP;
using ArtificialBuilder.Sharding;
using EDPFW;
using ArtificialBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>페르소나 DB (.psna) 수명·파일시스템·Vec 인프라만 담당. CRUD 는 Gateway 직결.</summary>
    public class AB_Persona_Db
    {
        // --- 초기화 ---

        /// <summary>EDP_Db_Engine 참조 저장 (실제 파일은 OpenAsync에서 열림).</summary>
        public void Initialize(EDP_Db_Engine _engine)
        {
            m_engine = _engine;
        }

        // --- 파일시스템 ---

        /// <summary>persona 디렉터리의 페르소나 이름 목록 (플랫 .psna + 디렉터리/core.psna 모두 감지).</summary>
        public List<string> GetPersonaNames()
        {
            Directory.CreateDirectory(PERSONA_DIR);
            HashSet<string> nameSet = new();

            // 플랫 파일: persona/*.psna
            string[] files = Directory.GetFiles(PERSONA_DIR, "*.psna");
            foreach (string f in files)
                nameSet.Add(Path.GetFileNameWithoutExtension(f));

            // 디렉터리 구조: persona/{name}/core.psna
            string[] dirs = Directory.GetDirectories(PERSONA_DIR);
            foreach (string d in dirs)
            {
                if (File.Exists(Path.Combine(d, "core.psna")))
                    nameSet.Add(Path.GetFileName(d));
            }

            List<string> names = new(nameSet);
            names.Sort(StringComparer.Ordinal);
            return names;
        }

        // --- 활성 페르소나 ---

        /// <summary>저장된 활성 페르소나(.active) 로드. 없으면 첫 페르소나.</summary>
        public async Task LoadActiveAsync()
        {
            Directory.CreateDirectory(PERSONA_DIR);
            List<string> names = GetPersonaNames();

            if (names.Count > 0)
            {
                string? savedName = null;
                if (File.Exists(ACTIVE_FILE))
                {
                    savedName = File.ReadAllText(ACTIVE_FILE).Trim();
                }

                string activeName = names[0];
                if (savedName != null && names.Contains(savedName))
                {
                    activeName = savedName;
                }

                await OpenAsync(activeName);
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("Persona", $"활성 페르소나: {activeName}");
            }
            else
            {
                ActiveName = null;
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("Persona", "페르소나 없음 → 초기 설정 필요");
            }
        }

        /// <summary>활성 페르소나 이름을 .active 파일에 저장.</summary>
        public void SaveActive(string _name)
        {
            Directory.CreateDirectory(PERSONA_DIR);
            File.WriteAllText(ACTIVE_FILE, _name);
            ActiveName = _name;
            ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("Persona", $"활성 페르소나 전환: {_name}");
        }

        // --- 열기/닫기 ---

        /// <summary>지정 이름의 페르소나 DB 열기 (기존 핸들은 닫음).</summary>
        public async Task OpenAsync(string _name)
        {
            if (Handle != 0)
            {
                if (m_shardManager != null)
                    await m_shardManager.CloseAllAsync();
                await m_engine.CloseAsync(Handle);
                Handle = 0;
                m_vecHandle = 0;
                m_shardRegistry = null;
                m_shardManager = null;
                m_personaDir = null;
            }

            Directory.CreateDirectory(PERSONA_DIR);
            AB_Persona_Context PersonaContextFactory(Microsoft.EntityFrameworkCore.DbContextOptions<AB_Persona_Context> _options)
            {
                return new AB_Persona_Context(_options);
            }

            // 단일 경로 — 디렉터리 구조 (persona/{name}/core.psna + 샤드). 마이그레이션 / 레거시 플랫 처리 없음.
            string dirPath = Path.Combine(PERSONA_DIR, _name);
            string corePath = Path.Combine(dirPath, "core.psna");
            Directory.CreateDirectory(dirPath);

            Handle = m_engine.OpenDatabase<AB_Persona_Context>(corePath, PersonaContextFactory);
            m_personaDir = dirPath;

            m_shardRegistry = new EDP_Shard_Registry();
            m_shardRegistry.Load(dirPath, _name);

            m_shardManager = new EDP_Shard_Manager();
            m_shardManager.Initialize(m_engine, m_shardRegistry);
            // Phase 4.4.d — POOL_PREFIX 샤드 (session_data_pool) 폐기.
            // Phase 4.6 — CTX_PREFIX 샤드 (context_records / context_history) 폐기.
            // 4 계층 storage ([[storage-layers]]) 가 정본.
            m_shardManager.RegisterOpener(EDP_Shard_Kind.Standalone,
                _path => m_engine.OpenDatabase<AB_Image_Shard_Context>(_path, _o => new AB_Image_Shard_Context(_o)));

            ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("Persona", $"페르소나 DB 열기 (샤딩): {corePath}, 샤드 {m_shardRegistry.AllShards.Count}개");

            ActiveName = _name;
            SaveActive(_name);
        }

        /// <summary>현재 페르소나 DB 닫기.</summary>
        public async Task CloseAsync()
        {
            if (Handle != 0)
            {
                if (m_shardManager != null)
                    await m_shardManager.CloseAllAsync();
                await m_engine.CloseAsync(Handle);
                Handle = 0;
                ActiveName = null;
                m_vecHandle = 0;
                m_shardRegistry = null;
                m_shardManager = null;
                m_personaDir = null;
                m_relations.Clear();
            }
        }

        // 스키마 마이그레이션은 framework/ResourceBuilder 가 담당 (별도 트랙).
        // 발매 전 단계는 버전 관리 / 마이그레이션 미적용 — EF 모델이 신규 DB 생성 시 그대로 반영, 기존 DB 호환 보장 없음.

        // --- 벡터 임베딩 인프라 (Gateway 직결용) ---

        private AB_Vec_Store? m_vecStore;
        private int m_vecHandle;

        /// <summary>벡터 저장소 (Gateway 직결용).</summary>
        public AB_Vec_Store? VecStore => m_vecStore;
        /// <summary>벡터 핸들 (0=미초기화).</summary>
        public int VecHandle => m_vecHandle;

        /// <summary>페르소나 DB에 벡터 저장소 초기화 (차원 지정).</summary>
        public void InitializeVec(int _dimensions)
        {
            if (Handle == 0) return;
            if (m_vecStore == null) m_vecStore = new AB_Vec_Store(m_engine);
            m_vecHandle = Handle;
            m_vecStore.Initialize(Handle, _dimensions);
        }

        /// <summary>벡터 저장소 초기화 여부.</summary>
        public bool IsVecInitialized()
        {
            if (m_vecStore == null || m_vecHandle == 0) return false;
            return m_vecStore.IsInitialized(m_vecHandle);
        }

        // --- 프로퍼티 ---

        /// <summary>현재 페르소나 DB 핸들 (0=닫힘).</summary>
        private int m_handle;
        public int Handle
        {
            get { return m_handle; }
            private set { m_handle = value; }
        }
        /// <summary>현재 활성 페르소나 이름.</summary>
        private string? m_activeName;
        public string? ActiveName
        {
            get { return m_activeName; }
            private set { m_activeName = value; }
        }

        // --- 샤딩 ---

        private EDP_Shard_Registry? m_shardRegistry;
        private EDP_Shard_Manager? m_shardManager;

        /// <summary>샤드 레지스트리. 샤딩 활성 시 non-null.</summary>
        public EDP_Shard_Registry? ShardRegistry => m_shardRegistry;
        /// <summary>샤드 매니저. 샤딩 활성 시 non-null.</summary>
        public EDP_Shard_Manager? ShardManager => m_shardManager;
        /// <summary>샤딩이 활성 상태인지 (디렉터리 구조).</summary>
        public bool IsSharded => m_shardRegistry != null;
        /// <summary>현재 페르소나 디렉터리 경로 (샤딩 시). null이면 레거시 플랫 파일.</summary>
        public string? PersonaDir => m_personaDir;

        private string? m_personaDir;

        // --- 관계 인덱스 ---

        private readonly AB_Relation_Manager m_relations = new();
        /// <summary>인메모리 관계 인덱스 (FK 대체).</summary>
        public AB_Relation_Manager Relations => m_relations;

        // --- 멤버 변수 ---

        private EDP_Db_Engine m_engine = null!;

        private const string PERSONA_DIR = "persona";
        private const string ACTIVE_FILE = "persona/.active";
    }
}
