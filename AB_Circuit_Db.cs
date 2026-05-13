using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Core;
using EDPFW;
using ArtificialBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>Circuit 목록 항목 (이름 + 외형 메타데이터).</summary>
    public class Circuit_List_Info : ArtificialBuilder_EDP.Core.AB_Object
    {
        /// <summary>Circuit 이름.</summary>
        private string m_name_ = "";
        public string Name_
        {
            get { return m_name_; }
            set { m_name_ = value; }
        }
        /// <summary>Circuit 타입 (chat/agent/image/sound 등).</summary>
        private string m_circuitType_ = "normal";
        public string CircuitType_
        {
            get { return m_circuitType_; }
            set { m_circuitType_ = value; }
        }
        /// <summary>아이콘 색상 hex.</summary>
        private string? m_iconColor_;
        public string? IconColor_
        {
            get { return m_iconColor_; }
            set { m_iconColor_ = value; }
        }
        /// <summary>아이콘 에셋 이름.</summary>
        private string? m_iconAsset_;
        public string? IconAsset_
        {
            get { return m_iconAsset_; }
            set { m_iconAsset_ = value; }
        }
        /// <summary>이름 색상 hex.</summary>
        private string? m_nameColor_;
        public string? NameColor_
        {
            get { return m_nameColor_; }
            set { m_nameColor_ = value; }
        }
        /// <summary>이름 폰트 에셋.</summary>
        private string? m_nameFontAsset_;
        public string? NameFontAsset_
        {
            get { return m_nameFontAsset_; }
            set { m_nameFontAsset_ = value; }
        }
        /// <summary>배경 색상 hex.</summary>
        private string? m_bgColor_;
        public string? BgColor_
        {
            get { return m_bgColor_; }
            set { m_bgColor_ = value; }
        }

        public override void Reset()
        {
            base.Reset();
            m_name_ = "";
            m_circuitType_ = "normal";
            m_iconColor_ = null;
            m_iconAsset_ = null;
            m_nameColor_ = null;
            m_nameFontAsset_ = null;
            m_bgColor_ = null;
        }
    }

    /// <summary>Circuit DB (.circuit) 수명·파일시스템·Vec 인프라만 담당. CRUD 는 Gateway 직결.</summary>
    public class AB_Circuit_Db : ArtificialBuilder_EDP.Core.AB_Object
    {
        // --- 확장명 ---

        /// <summary>Circuit 통합 확장자. 타입과 무관하게 모두 .circuit.</summary>
        public const string CIRCUIT_EXTENSION = ".circuit";

        /// <summary>지원하는 모든 Circuit 확장자 (단일).</summary>
        public static readonly string[] ALL_EXTENSIONS = { CIRCUIT_EXTENSION };

        /// <summary>Circuit 타입 → 확장자 (통합: 항상 .circuit). 타입은 파일 내부에 저장됨.</summary>
        public static string GetExtensionForType(string _type)
        {
            return CIRCUIT_EXTENSION;
        }

        // --- 초기화 ---

        /// <summary>AB_DB 참조 저장 (실제 파일은 OpenAsync에서).</summary>
        public void Initialize(AB_DB _engine)
        {
            m_engine = _engine;
        }

        // --- 파일시스템 ---

        /// <summary>circuit 디렉터리의 Circuit 이름 목록 (모든 확장자).</summary>
        public List<string> GetCircuitNames()
        {
            Directory.CreateDirectory(CIRCUIT_DIR);
            HashSet<string> seen = new();
            List<string> names = new();
            foreach (string ext in ALL_EXTENSIONS)
            {
                foreach (string f in Directory.GetFiles(CIRCUIT_DIR, $"*{ext}"))
                {
                    string name = Path.GetFileNameWithoutExtension(f);
                    if (seen.Add(name))
                    {
                        names.Add(name);
                    }
                }
            }
            names.Sort(StringComparer.Ordinal);
            return names;
        }

        /// <summary>이름 + 타입 튜플 목록 (확장자로 타입 판별).</summary>
        public List<(string Name, string CircuitType)> GetCircuitNamesWithType()
        {
            Directory.CreateDirectory(CIRCUIT_DIR);
            List<(string, string)> result = new();
            foreach (string ext in ALL_EXTENSIONS)
            {
                foreach (string file in Directory.GetFiles(CIRCUIT_DIR, $"*{ext}"))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    // 타입은 파일 내부 circuit_settings.circuit_type에 저장됨. 목록에서는 기본값 "chat".
                    result.Add((name, "chat"));
                }
            }
            static int CompareTupleByName((string, string) _a, (string, string) _b)
            {
                return string.Compare(_a.Item1, _b.Item1, StringComparison.Ordinal);
            }
            result.Sort(CompareTupleByName);
            return result;
        }

        /// <summary>전체 Circuit + 외형 메타데이터 조회 (비활성은 ReadOnly로 직접 SQL).</summary>
        public async Task<List<Circuit_List_Info>> GetCircuitListInfoAsync()
        {
            var namesWithType = GetCircuitNamesWithType();
            List<Circuit_List_Info> result = new();

            foreach (var (name, extType) in namesWithType)
            {
                Circuit_List_Info info = AB_Engine.GetService<AB_Pool>().AcquireObject<Circuit_List_Info>();
                info.Name_ = name;
                info.CircuitType_ = extType;

                try
                {
                    // 현재 활성 Circuit은 기존 핸들에서 직접 읽기
                    if (Handle != 0 && name == ActiveName)
                    {
                        var settings = await m_engine.GetByIdAsync<AB_Circuit_Settings_Model>(Handle, 1L);
                        if (settings != null)
                        {
                            if (!string.IsNullOrEmpty(settings.CircuitType_))
                                info.CircuitType_ = settings.CircuitType_;
                            info.IconColor_ = settings.IconColor_;
                            info.IconAsset_ = settings.IconAsset_;
                            info.NameColor_ = settings.NameColor_;
                            info.NameFontAsset_ = settings.NameFontAsset_;
                            info.BgColor_ = settings.BgColor_;
                        }
                    }
                    else
                    {
                        // 비활성 Circuit은 ReadOnly 연결로 읽기 (파일 덮어쓰기 방지)
                        string? dbPath = FindCircuitFile(name);
                        if (dbPath != null)
                        {
                            using var conn = new Microsoft.Data.Sqlite.SqliteConnection(
                                $"Data Source={dbPath};Mode=ReadOnly");
                            await conn.OpenAsync();
                            using var cmd = conn.CreateCommand();
                            cmd.CommandText = "SELECT circuit_type, icon_color, icon_asset, name_color, name_font_asset, bg_color FROM circuit_settings WHERE id = 'settings' LIMIT 1";
                            using var reader = await cmd.ExecuteReaderAsync();
                            if (await reader.ReadAsync())
                            {
                                string? circuitType = reader.IsDBNull(0) ? null : reader.GetString(0);
                                if (!string.IsNullOrEmpty(circuitType)) info.CircuitType_ = circuitType;
                                info.IconColor_ = reader.IsDBNull(1) ? null : reader.GetString(1);
                                info.IconAsset_ = reader.IsDBNull(2) ? null : reader.GetString(2);
                                info.NameColor_ = reader.IsDBNull(3) ? null : reader.GetString(3);
                                info.NameFontAsset_ = reader.IsDBNull(4) ? null : reader.GetString(4);
                                info.BgColor_ = reader.IsDBNull(5) ? null : reader.GetString(5);
                            }
                        }
                    }
                }
                catch { }

                result.Add(info);
            }

            return result;
        }

        // --- 열기/닫기 ---

        /// <summary>이름으로 Circuit DB 열기 (확장자 자동 탐지).</summary>
        public async Task OpenAsync(string _circuitName)
        {
            // Path traversal defense
            string safeName = Path.GetFileNameWithoutExtension(_circuitName);
            if (string.IsNullOrEmpty(safeName) || safeName.Contains("..") || safeName.Contains('/') || safeName.Contains('\\'))
                throw new ArgumentException($"Invalid circuit name: {_circuitName}");

            if (Handle != 0)
            {
                await m_engine.CloseAsync(Handle);
                Handle = 0;
            }

            Directory.CreateDirectory(CIRCUIT_DIR);
            string? existingPath = FindCircuitFile(safeName);
            string dbPath = existingPath ?? $"{CIRCUIT_DIR}/{safeName}.circuit";
            AB_Circuit_Db_Context CircuitContextFactory(Microsoft.EntityFrameworkCore.DbContextOptions<AB_Circuit_Db_Context> _options)
            {
                return new AB_Circuit_Db_Context(_options);
            }
            Handle = m_engine.OpenDatabase<AB_Circuit_Db_Context>(
                dbPath, CircuitContextFactory);
            ActiveName = _circuitName;
            // Circuit 타입은 파일 내부 circuit_settings.circuit_type에서 읽음 (엔진 직결)
            try
            {
                var settings = await m_engine.GetByIdAsync<AB_Circuit_Settings_Model>(Handle, 1L);
                ActiveType = settings?.CircuitType_ ?? "chat";
            }
            catch { ActiveType = "chat"; }
            ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("Circuit", $"Circuit DB 열기: {dbPath}");
            // CircuitRegistry 세대 교체 + 이벤트 발화는 AB_Actions.OpenCircuitAsync 파사드 책임.
            // 직접 AB_Board.Circuit.OpenAsync 호출 금지 — AB_Actions 경유 ([[feedback_no_side_patches]]).
        }

        /// <summary>Circuit 타입을 명시해 새 DB 열기.</summary>
        public async Task OpenAsync(string _circuitName, string _circuitType)
        {
            if (Handle != 0)
            {
                await m_engine.CloseAsync(Handle);
                Handle = 0;
            }

            Directory.CreateDirectory(CIRCUIT_DIR);
            string ext = GetExtensionForType(_circuitType);
            string dbPath = $"{CIRCUIT_DIR}/{_circuitName}{ext}";
            AB_Circuit_Db_Context CircuitContextFactory(Microsoft.EntityFrameworkCore.DbContextOptions<AB_Circuit_Db_Context> _options)
            {
                return new AB_Circuit_Db_Context(_options);
            }
            Handle = m_engine.OpenDatabase<AB_Circuit_Db_Context>(
                dbPath, CircuitContextFactory);
            ActiveName = _circuitName;
            ActiveType = _circuitType;
            ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("Circuit", $"Circuit DB 열기: {dbPath}");
            // 위와 동일 — Swap 은 AB_Actions.OpenCircuitWithTypeAsync 담당.
        }

        /// <summary>현재 Circuit DB 닫기.</summary>
        public async Task CloseAsync()
        {
            if (Handle != 0)
            {
                await m_engine.CloseAsync(Handle);
                Handle = 0;
                ActiveName = null;
            }
        }

        // --- FindCircuitFile ---

        /// <summary>이름으로 실제 Circuit 파일 경로 탐색 (확장자 자동).</summary>
        public static string? FindCircuitFile(string _name)
        {
            string safeName = Path.GetFileNameWithoutExtension(_name);
            if (string.IsNullOrEmpty(safeName) || safeName != _name) return null;

            foreach (string ext in ALL_EXTENSIONS)
            {
                string path = Path.Combine(CIRCUIT_DIR, $"{_name}{ext}");
                if (File.Exists(path)) return path;
            }
            return null;
        }

        // 스키마 마이그레이션은 framework/ResourceBuilder 가 담당 (별도 트랙).
        // 발매 전 단계는 버전 관리 / 마이그레이션 미적용 — EF 모델이 신규 DB 생성 시 그대로 반영, 기존 DB 호환 보장 없음.

        // --- Vec 인프라 (Gateway 직결용) ---

        private AB_Vec_Store? m_vecStore;
        private int m_vecHandle;

        /// <summary>벡터 저장소 (Gateway 직결용).</summary>
        public AB_Vec_Store? VecStore => m_vecStore;
        /// <summary>벡터 핸들 (0=미초기화).</summary>
        public int VecHandle => m_vecHandle;

        /// <summary>벡터 저장소 초기화 (차원 지정).</summary>
        public void InitializeVec(int _handle, int _dimensions)
        {
            if (m_vecStore == null) m_vecStore = new AB_Vec_Store(m_engine);
            m_vecHandle = _handle;
            m_vecStore.Initialize(_handle, _dimensions);
        }

        // --- 프로퍼티 ---

        /// <summary>현재 Circuit DB 핸들 (0=닫힘).</summary>
        private int m_handle;
        public int Handle
        {
            get { return m_handle; }
            private set { m_handle = value; }
        }
        /// <summary>현재 활성 Circuit 이름.</summary>
        private string? m_activeName;
        public string? ActiveName
        {
            get { return m_activeName; }
            private set { m_activeName = value; }
        }
        /// <summary>현재 활성 Circuit 타입.</summary>
        private string m_activeType = "chat";
        public string ActiveType
        {
            get { return m_activeType; }
            private set { m_activeType = value; }
        }

        // --- 멤버 변수 ---

        private AB_DB m_engine = null!;

        private const string CIRCUIT_DIR = "circuit";
    }
}
