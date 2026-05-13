using ArtificialBuilder_EDP;
using EDPFW;
using ArtificialBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArtificialBuilder_EDP.Core;

namespace ArtificialBuilder
{
    /// <summary>Logic DB (.alogic) 수명·파일시스템 보관소. per-logic SQLite. 한 Logic Id (long) = 1 파일.
    /// 2026-05-11 — string Guid 폐기. 파일명 = "{long}.alogic" (예: 1.alogic). 사용자 정본 "string 키 보면 역겨움".</summary>
    public class AB_Logic_Db : ArtificialBuilder_EDP.Core.AB_Object
    {
        // --- 확장명 ---

        /// <summary>Logic 통합 확장자.</summary>
        public const string LOGIC_EXTENSION = ".alogic";

        /// <summary>Logic 디렉터리.</summary>
        private const string LOGIC_DIR = "logic";

        // --- 초기화 ---

        /// <summary>AB_DB 참조 저장 (실제 파일은 OpenAsync 에서).</summary>
        public void Initialize(AB_DB _engine)
        {
            m_engine = _engine;
        }

        // --- 파일시스템 ---

        /// <summary>logic 디렉터리의 Logic Id (long) 목록.</summary>
        public List<long> GetLogicUuids()
        {
            Directory.CreateDirectory(LOGIC_DIR);
            HashSet<long> seen = new();
            List<long> uuids = new();
            foreach (string f in Directory.GetFiles(LOGIC_DIR, $"*{LOGIC_EXTENSION}"))
            {
                string nameOnly = Path.GetFileNameWithoutExtension(f);
                if (!long.TryParse(nameOnly, out long uuid))
                {
                    ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("LogicDb", $"GetLogicUuids — 비-long 파일명 skip: {nameOnly}");
                    continue;
                }
                if (seen.Add(uuid))
                {
                    uuids.Add(uuid);
                }
            }
            uuids.Sort();
            return uuids;
        }

        /// <summary>신규 Logic 파일 생성. uuid 0 이면 새 long 발급. 동일 uuid 가 이미 있으면 0 반환.</summary>
        public async Task<long> CreateLogicFileAsync(long _uuid = 0L, string? _name = null)
        {
            long uuid = _uuid;
            if (uuid == 0L) uuid = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
            Directory.CreateDirectory(LOGIC_DIR);
            string dbPath = Path.Combine(LOGIC_DIR, $"{uuid}{LOGIC_EXTENSION}");
            if (File.Exists(dbPath))
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("LogicDb", $"CreateLogicFile — already exists: {dbPath}");
                return 0L;
            }
            int handle = m_engine.OpenDatabase<AB_Logic_Db_Context>(
                dbPath, _options => new AB_Logic_Db_Context(_options));
            try
            {
                AB_Logic_Meta_Model meta = new()
                {
                    Id_ = 1,
                    LogicUuid_ = uuid,
                    DisplayName_ = _name ?? "",
                };
                await AB_Board.Db.AddAsync(handle, meta);
                await AB_Board.Db.SaveChangesAsync(handle);
            }
            finally
            {
                await m_engine.CloseAsync(handle);
            }
            ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("LogicDb", $"CreateLogicFile — uuid={uuid} name={_name}");
            return uuid;
        }

        /// <summary>Logic 파일 삭제. 활성 open 시 차단.</summary>
        public bool DeleteLogicFile(long _uuid)
        {
            if (_uuid == 0L)
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("LogicDb", $"DeleteLogicFile — invalid uuid: 0");
                return false;
            }
            if (Handle != 0 && ActiveUuid == _uuid)
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("LogicDb", $"DeleteLogicFile — uuid {_uuid} is active, refusing");
                return false;
            }
            string dbPath = Path.Combine(LOGIC_DIR, $"{_uuid}{LOGIC_EXTENSION}");
            if (!File.Exists(dbPath))
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("LogicDb", $"DeleteLogicFile — not found: {dbPath}");
                return false;
            }
            File.Delete(dbPath);
            string shm = dbPath + "-shm";
            if (File.Exists(shm)) File.Delete(shm);
            string wal = dbPath + "-wal";
            if (File.Exists(wal)) File.Delete(wal);
            ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("LogicDb", $"DeleteLogicFile — uuid={_uuid}");
            return true;
        }

        /// <summary>logic 디렉터리 scan + 각 Logic Id 의 meta name 동반 list.</summary>
        public async Task<List<AB_Logic_Library_Item>> GetLogicLibraryInfoAsync()
        {
            List<AB_Logic_Library_Item> items = new();
            List<long> uuids = GetLogicUuids();
            foreach (long uuid in uuids)
            {
                string dbPath = Path.Combine(LOGIC_DIR, $"{uuid}{LOGIC_EXTENSION}");
                DateTime updatedAt = File.GetLastWriteTimeUtc(dbPath);
                string name = "";
                if (Handle != 0 && ActiveUuid == uuid)
                {
                    AB_Logic_Meta_Model? meta = await AB_Board.Db.GetByIdAsync<AB_Logic_Meta_Model>(Handle, 1L);
                    if (meta != null) name = meta.DisplayName_ ?? "";
                }
                else
                {
                    int h = m_engine.OpenDatabase<AB_Logic_Db_Context>(
                        dbPath, _options => new AB_Logic_Db_Context(_options));
                    try
                    {
                        AB_Logic_Meta_Model? meta = await AB_Board.Db.GetByIdAsync<AB_Logic_Meta_Model>(h, 1L);
                        if (meta != null) name = meta.DisplayName_ ?? "";
                    }
                    finally
                    {
                        await m_engine.CloseAsync(h);
                    }
                }
                var item = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Logic_Library_Item>();
                item.Uuid_ = uuid;
                item.Name_ = name;
                item.UpdatedAt_ = updatedAt;
                items.Add(item);
            }
            return items;
        }

        // --- 열기/닫기 ---

        /// <summary>Logic Id (long) 로 Logic DB 열기.</summary>
        public async Task OpenAsync(long _logicUuid)
        {
            if (_logicUuid == 0L)
            {
                throw new ArgumentException($"Invalid logic uuid: 0");
            }

            if (Handle != 0)
            {
                await m_engine.CloseAsync(Handle);
                Handle = 0;
            }

            Directory.CreateDirectory(LOGIC_DIR);
            string dbPath = $"{LOGIC_DIR}/{_logicUuid}{LOGIC_EXTENSION}";
            AB_Logic_Db_Context LogicContextFactory(Microsoft.EntityFrameworkCore.DbContextOptions<AB_Logic_Db_Context> _options)
            {
                return new AB_Logic_Db_Context(_options);
            }
            Handle = m_engine.OpenDatabase<AB_Logic_Db_Context>(
                dbPath, LogicContextFactory);
            ActiveUuid = _logicUuid;
            ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("LogicDb", $"Logic DB 열기: {dbPath}");
        }

        /// <summary>현재 Logic DB 닫기.</summary>
        public async Task CloseAsync()
        {
            if (Handle != 0)
            {
                await m_engine.CloseAsync(Handle);
                Handle = 0;
                ActiveUuid = 0L;
            }
        }

        /// <summary>Logic Id (long) 로 실제 Logic 파일 경로 탐색.</summary>
        public static string? FindLogicFile(long _uuid)
        {
            if (_uuid == 0L)
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

        /// <summary>현재 활성 Logic Id (long, 0 = 미활성).</summary>
        private long m_activeUuid;
        public long ActiveUuid
        {
            get { return m_activeUuid; }
            private set { m_activeUuid = value; }
        }

        // --- 멤버 변수 ---

        private AB_DB m_engine = null!;
    }
}
