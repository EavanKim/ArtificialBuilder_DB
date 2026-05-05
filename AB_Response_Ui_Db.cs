using ArtificialBuilder_EDP;
using EDPFW;
using ArtificialBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>Response UI DB (.aresui) 수명·파일시스템 보관소. per-response-ui SQLite. UUID = 1 파일.</summary>
    public class AB_Response_Ui_Db
    {
        // --- 확장명 ---

        public const string RESPONSE_UI_EXTENSION = ".aresui";

        private const string RESPONSE_UI_DIR = "response_ui";

        // --- 초기화 ---

        public void Initialize(EDP_Db_Engine _engine)
        {
            m_engine = _engine;
        }

        // --- 파일시스템 ---

        public List<string> GetResponseUiUuids()
        {
            Directory.CreateDirectory(RESPONSE_UI_DIR);
            HashSet<string> seen = new();
            List<string> uuids = new();
            foreach (string f in Directory.GetFiles(RESPONSE_UI_DIR, $"*{RESPONSE_UI_EXTENSION}"))
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

        public async Task OpenAsync(string _uuid)
        {
            string safeUuid = Path.GetFileNameWithoutExtension(_uuid);
            if (string.IsNullOrEmpty(safeUuid) || safeUuid.Contains("..") || safeUuid.Contains('/') || safeUuid.Contains('\\'))
            {
                throw new ArgumentException($"Invalid response_ui uuid: {_uuid}");
            }

            if (Handle != 0)
            {
                await m_engine.CloseAsync(Handle);
                Handle = 0;
            }

            Directory.CreateDirectory(RESPONSE_UI_DIR);
            string dbPath = $"{RESPONSE_UI_DIR}/{safeUuid}{RESPONSE_UI_EXTENSION}";
            AB_Response_Ui_Db_Context ResponseUiContextFactory(Microsoft.EntityFrameworkCore.DbContextOptions<AB_Response_Ui_Db_Context> _options)
            {
                return new AB_Response_Ui_Db_Context(_options);
            }
            Handle = m_engine.OpenDatabase<AB_Response_Ui_Db_Context>(
                dbPath, ResponseUiContextFactory);
            ActiveUuid = safeUuid;
            AB_Log.Info("ResponseUiDb", $"Response UI DB 열기: {dbPath}");
        }

        public async Task CloseAsync()
        {
            if (Handle != 0)
            {
                await m_engine.CloseAsync(Handle);
                Handle = 0;
                ActiveUuid = null;
            }
        }

        public static string? FindResponseUiFile(string _uuid)
        {
            string safeUuid = Path.GetFileNameWithoutExtension(_uuid);
            if (string.IsNullOrEmpty(safeUuid) || safeUuid != _uuid)
            {
                return null;
            }
            string path = Path.Combine(RESPONSE_UI_DIR, $"{_uuid}{RESPONSE_UI_EXTENSION}");
            if (File.Exists(path))
            {
                return path;
            }
            return null;
        }

        // --- 프로퍼티 ---

        private int m_handle;
        public int Handle
        {
            get { return m_handle; }
            private set { m_handle = value; }
        }

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
