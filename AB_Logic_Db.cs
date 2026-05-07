using ArtificialBuilder_EDP;
using EDPFW;
using ArtificialBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>Logic DB (.alogic) 수명·파일시스템 보관소. per-logic SQLite. 한 Logic UUID = 1 파일.</summary>
    public class AB_Logic_Db
    {
        // --- 확장명 ---

        /// <summary>Logic 통합 확장자.</summary>
        public const string LOGIC_EXTENSION = ".alogic";

        /// <summary>Logic 디렉터리.</summary>
        private const string LOGIC_DIR = "logic";

        // --- 초기화 ---

        /// <summary>EDP_Db_Engine 참조 저장 (실제 파일은 OpenAsync 에서).</summary>
        public void Initialize(EDP_Db_Engine _engine)
        {
            m_engine = _engine;
        }

        // --- 파일시스템 ---

        // TODO(main-tabs-and-package-system sub 2): 로직 라이브러리 — CreateLogicFile / DeleteLogicFile 신설.
        // CreateLogicFile = UUID 생성 + 빈 .alogic 파일 (EF Core schema apply 포함).
        // DeleteLogicFile = .alogic + .alogic-shm 등 일괄 안전 삭제 (활성 open 시 차단).
        // GetLogicLibraryInfoAsync (Db_Proxy 측) = UUID + meta name 동반 list.
        // plans/doing/main-tabs-and-package-system/sub-2-logic-library-screen.md
        /// <summary>logic 디렉터리의 Logic UUID 목록.</summary>
        public List<string> GetLogicUuids()
        {
            Directory.CreateDirectory(LOGIC_DIR);
            HashSet<string> seen = new();
            List<string> uuids = new();
            foreach (string f in Directory.GetFiles(LOGIC_DIR, $"*{LOGIC_EXTENSION}"))
            {
                string uuid = Path.GetFileNameWithoutExtension(f);
                if (seen.Add(uuid))
                {
                    uuids.Add(uuid);
                }
            }
            uuids.Sort(StringComparer.Ordinal);
            return uuids;
        }

        // --- 열기/닫기 ---

        /// <summary>UUID 로 Logic DB 열기.</summary>
        public async Task OpenAsync(string _logicUuid)
        {
            string safeUuid = Path.GetFileNameWithoutExtension(_logicUuid);
            if (string.IsNullOrEmpty(safeUuid) || safeUuid.Contains("..") || safeUuid.Contains('/') || safeUuid.Contains('\\'))
            {
                throw new ArgumentException($"Invalid logic uuid: {_logicUuid}");
            }

            if (Handle != 0)
            {
                await m_engine.CloseAsync(Handle);
                Handle = 0;
            }

            Directory.CreateDirectory(LOGIC_DIR);
            string dbPath = $"{LOGIC_DIR}/{safeUuid}{LOGIC_EXTENSION}";
            AB_Logic_Db_Context LogicContextFactory(Microsoft.EntityFrameworkCore.DbContextOptions<AB_Logic_Db_Context> _options)
            {
                return new AB_Logic_Db_Context(_options);
            }
            Handle = m_engine.OpenDatabase<AB_Logic_Db_Context>(
                dbPath, LogicContextFactory);
            ActiveUuid = safeUuid;
            AB_Log.Info("LogicDb", $"Logic DB 열기: {dbPath}");
        }

        /// <summary>현재 Logic DB 닫기.</summary>
        public async Task CloseAsync()
        {
            if (Handle != 0)
            {
                await m_engine.CloseAsync(Handle);
                Handle = 0;
                ActiveUuid = null;
            }
        }

        /// <summary>UUID 로 실제 Logic 파일 경로 탐색.</summary>
        public static string? FindLogicFile(string _uuid)
        {
            string safeUuid = Path.GetFileNameWithoutExtension(_uuid);
            if (string.IsNullOrEmpty(safeUuid) || safeUuid != _uuid)
            {
                return null;
            }
            string path = Path.Combine(LOGIC_DIR, $"{_uuid}{LOGIC_EXTENSION}");
            if (File.Exists(path))
            {
                return path;
            }
            return null;
        }

        // --- 프로퍼티 ---

        /// <summary>현재 Logic DB 핸들 (0=닫힘).</summary>
        private int m_handle;
        public int Handle
        {
            get { return m_handle; }
            private set { m_handle = value; }
        }

        /// <summary>현재 활성 Logic UUID.</summary>
        private string? m_activeUuid;
        public string? ActiveUuid
        {
            get { return m_activeUuid; }
            private set { m_activeUuid = value; }
        }

        // --- 멤버 변수 ---

        private EDP_Db_Engine m_engine = null!;
    }
}
