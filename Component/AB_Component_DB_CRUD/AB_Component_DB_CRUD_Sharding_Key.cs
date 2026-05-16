using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EDPFW;

namespace ArtificialBuilder.DB.Component
{
    // 콘크리트 — 키 매개 핸들 라우팅 CRUD. Node 페이로드 (폴더 안 키 샤딩 파일 N). AB_Object_DB_Sharding_Key 안 m_crud instance.
    // 본 Component = row 안 키 데이터 (예 turn_id range / payload_kind / sub-bucket) 매개 shard_key 산정 → AB_Component_DB_File_Sharding.AcquireHandle_(shard_key) → EDP_Db_Transaction 호출 (round 2 본체).
    // shard_key 산정 정책 D2 = round 2 결재.
    // round 1 = 시그니처 + stub body.
    public class AB_Component_DB_CRUD_Sharding_Key : AB_Component_DB_CRUD
    {
        public AB_Component_DB_CRUD_Sharding_Key()
        {
        }

        public override Task AddAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Sharding_Key.AddAsync_: round 2b 본체");
        }

        public override Task<T?> GetByIdAsync_<T>(EDP_Db_Transaction _txn, long _id) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Sharding_Key.GetByIdAsync_: round 2b 본체");
        }

        public override Task<List<T>> FindAsync_<T>(EDP_Db_Transaction _txn, Expression<Func<T, bool>> _predicate) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Sharding_Key.FindAsync_: round 2b 본체");
        }

        public override Task UpdateAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Sharding_Key.UpdateAsync_: round 2b 본체");
        }

        public override Task RemoveAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Sharding_Key.RemoveAsync_: round 2b 본체");
        }

        public override void Dispose()
        {
        }
    }
}
