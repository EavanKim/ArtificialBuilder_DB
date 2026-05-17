using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EDPFW;

namespace ArtificialBuilder.DB.Component
{
    // 콘크리트 — 폴더 + 키 매개 파일 N lifecycle. Sharding_Key (Result 노드 폴더 안 data_key range) / Sharding_History (Turn 폴더 안 turn_id range) 양립.
    // canon § "Object / Component 다형성 조립 패턴" 정합. AB_Object_DB_Sharding_Key / _Sharding_History 안 m_file instance 매개 attach.
    //
    // 폴더 1 = 콘크리트 Object 결정 (Sharding_Key = `runtime/result/{circuit_id}/{node_number}/` / Sharding_History = `runtime/turn/{circuit_id}/`).
    // 폴더 안 파일 = `{shard_key}.db` (Sharding_Key = data_key / 100 / Sharding_History = turn_id / 100). 첫 요청 시점 lazy open.
    // 본 Component 자체 = 키 의미 모름 (long 만) — caller (CRUD / Manager) 매개 shard_key 산정.
    // FK 최저화 룰 매개 cross-file 무결성 = 어플리케이션 책임. 본 Component = 단일 shard 파일 안 batch 보장 만.
    public class AB_Component_DB_File_Sharding : AB_Component_DB_File
    {
        // 폴더 경로 (Open_ 매개 주입). 폴더 안 파일 = `{shard_key}.db` 패턴.
        private string m_folder_path;

        // 키 → EDP_Db_Engine handle 캐시. 첫 요청 매개 lazy open.
        private readonly Dictionary<long, int> m_shards;

        // Open_ 매개 주입 engine ref — Close_ / BeginTransactionAsync_ / SyncDirtyToFile_ / AcquireHandle_ 시점 사용.
        private EDP_Db_Engine? m_engine;

        // Open_ 매개 주입 opener delegate (콘크리트 Object 가 TContext 결정) — AcquireHandle_ 매개 신규 shard 파일 open.
        private Func<EDP_Db_Engine, string, int>? m_opener;

        public AB_Component_DB_File_Sharding()
        {
            m_folder_path = string.Empty;
            m_shards = new Dictionary<long, int>();
            m_engine = null;
            m_opener = null;
        }

        // 폴더 1 = caller (Object) 결정. Directory.CreateDirectory 매개 폴더 보장 후 engine / opener 보관.
        // 파일 open = lazy (AcquireHandle_ 매개 첫 시점). 본 Open_ 자체는 파일 IO 무.
        public override void Open_(string _root_path, EDP_Db_Engine _engine, Func<EDP_Db_Engine, string, int> _opener)
        {
            if (m_engine != null)
            {
                throw new InvalidOperationException("AB_Component_DB_File_Sharding.Open_: 이미 open 상태 folder=" + m_folder_path);
            }
            Directory.CreateDirectory(_root_path);
            m_folder_path = _root_path;
            m_engine = _engine;
            m_opener = _opener;
        }

        // m_shards 안 모든 handle cascade close. 이후 재 Open_ 가능 상태 진입.
        public override void Close_()
        {
            if (m_engine == null)
            {
                m_shards.Clear();
                return;
            }
            foreach (int handle in m_shards.Values)
            {
                m_engine.CloseAsync(handle).AsTask().GetAwaiter().GetResult();
            }
            m_shards.Clear();
            m_folder_path = string.Empty;
            m_engine = null;
            m_opener = null;
        }

        // Sharding family = shard_key overload 사용. no-arg = NotSupported.
        public override Task<EDP_Db_Transaction> BeginTransactionAsync_()
        {
            throw new NotSupportedException("AB_Component_DB_File_Sharding.BeginTransactionAsync_(): Sharding = shard_key overload 사용 (folder=" + m_folder_path + ")");
        }

        // shard_key 매개 handle 선택 후 BeginTransactionAsync. caller (CRUD_Sharding_*) 매개 shard_key 산정 후 호출.
        public override Task<EDP_Db_Transaction> BeginTransactionAsync_(long _shard_key)
        {
            if (m_engine == null)
            {
                throw new InvalidOperationException("AB_Component_DB_File_Sharding.BeginTransactionAsync_: 미 open (m_engine == null)");
            }
            int handle = AcquireHandle_(_shard_key);
            return m_engine.BeginTransactionAsync(handle);
        }

        // 키 매개 handle 발급 (lazy). hit = 캐시 반환 / miss = opener 매개 `{folder}/{shard_key}.db` 신규 open + 캐시.
        // caller (Object 매개 직접 호출 site 결재 매개) — 본 메서드 자체 = public 매개 노출.
        public int AcquireHandle_(long _shard_key)
        {
            if (m_engine == null || m_opener == null)
            {
                throw new InvalidOperationException("AB_Component_DB_File_Sharding.AcquireHandle_: 미 open (engine/opener null)");
            }
            if (m_shards.TryGetValue(_shard_key, out int existing))
            {
                return existing;
            }
            string file_path = Path.Combine(m_folder_path, _shard_key.ToString() + ".db");
            int handle = m_opener(m_engine, file_path);
            if (handle == 0)
            {
                throw new InvalidOperationException("AB_Component_DB_File_Sharding.AcquireHandle_: 핸들 발급 실패 file=" + file_path);
            }
            m_shards[_shard_key] = handle;
            return handle;
        }

        // engine 전체 dirty entry sweep — m_shards 안 entry 도 포함.
        public override void SyncDirtyToFile_()
        {
            if (m_engine == null)
            {
                return;
            }
            m_engine.SyncDirtyToFile();
        }

        public override void Dispose()
        {
            Close_();
        }
    }
}
