using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EDPFW;

namespace ArtificialBuilder.DB.Component
{
    // 콘크리트 — 폴더 + 키 매개 파일 N lifecycle. Sharding_Key (Node 폴더) / Sharding_History (Turn 폴더) 양립.
    // canon § "Object / Component 다형성 조립 패턴" 정합. AB_Object_DB_Sharding_Key / _Sharding_History 안 m_file instance 매개 attach.
    //
    // 폴더 1 = caller (Sharding_Key = node_id / Sharding_History = circuit_id). 폴더 안 파일 = lazy open (첫 row 매개 round 2 본체).
    // 본 Component 자체 = 키 의미 모름 (long 만) — 콘크리트 CRUD 측 매개 key 산정 + 핸들 요청.
    //
    // round 1 (skeleton) = field + 시그니처 + stub body 만. Open_ / lazy open 본체 = round 2.
    public class AB_Component_DB_File_Sharding : AB_Component_DB_File
    {
        // 폴더 경로 (Open_ 매개 주입). 폴더 안 파일 = `{shard_key}.db` 패턴.
        private string m_folder_path;

        // 키 → EDP_Db_Engine handle 캐시. 첫 요청 매개 lazy open.
        private readonly Dictionary<long, int> m_shards;

        // m_engine ref = round 2 본체 진입 시 추가.
        public AB_Component_DB_File_Sharding()
        {
            m_folder_path = string.Empty;
            m_shards = new Dictionary<long, int>();
        }

        // round 2b 본체 — Directory.CreateDirectory(_root_path) + m_engine / m_folder_path 보관 + m_opener 저장. 파일 open = lazy (AcquireHandle_ 매개 첫 시점).
        public override void Open_(string _root_path, EDP_Db_Engine _engine, Func<EDP_Db_Engine, string, int> _opener)
        {
            throw new NotImplementedException("AB_Component_DB_File_Sharding.Open_: round 2b 본체");
        }

        // round 2b 본체 — m_shards 안 모든 handle EDP_Db_Engine.CloseAsync cascade.
        public override void Close_()
        {
            throw new NotImplementedException("AB_Component_DB_File_Sharding.Close_: round 2b 본체");
        }

        // round 2b 본체 — Sharding 은 row 매개 handle 선택 후 BeginTransactionAsync 호출. caller (CRUD_Sharding_*) 매개 row 정보 필요 → 본 시그니처 미부합.
        // Sharding family = AcquireHandle_(shard_key) 매개 handle 노출 + caller 측 BeginTransactionAsync 직접 호출 패턴 (round 2b 결재).
        public override Task<EDP_Db_Transaction> BeginTransactionAsync_()
        {
            throw new NotImplementedException("AB_Component_DB_File_Sharding.BeginTransactionAsync_: round 2b 본체 — Sharding family = row 매개 handle 선택 패턴 결재");
        }

        // round 2b 본체 — EDP_Db_Engine.SyncDirtyToFile 매개 (engine = 전 entry 매개 iterate, m_shards 안 entry 도 포함).
        public override void SyncDirtyToFile_()
        {
            throw new NotImplementedException("AB_Component_DB_File_Sharding.SyncDirtyToFile_: round 2b 본체");
        }

        // 키 매개 handle 발급 (lazy). round 2 본체 = m_shards lookup hit 시 반환 / miss 시 EDP_Db_Engine.OpenDatabase<TContext>($"{m_folder_path}/{_shard_key}.db", factory) → handle 캐시 + 반환.
        // CRUD 콘크리트 측 매개 호출.
        public int AcquireHandle_(long _shard_key)
        {
            throw new NotImplementedException("AB_Component_DB_File_Sharding.AcquireHandle_: round 2 본체");
        }

        public override void Dispose()
        {
            m_shards.Clear();
            m_folder_path = string.Empty;
        }
    }
}
