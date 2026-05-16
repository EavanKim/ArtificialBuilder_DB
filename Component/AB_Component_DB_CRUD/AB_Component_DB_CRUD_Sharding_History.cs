using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArtificialBuilder.DB.Component
{
    // 콘크리트 — 시계열 매개 핸들 라우팅 CRUD. Turn 히스토리 (폴더 안 시계열 샤딩 파일 N). AB_Object_DB_Sharding_History 안 m_crud instance.
    // 본 Component = row 안 시계열 데이터 (예 turn_id / started_at) 매개 turn_bucket 산정 → AB_Component_DB_File_Sharding.AcquireHandle_(turn_bucket) → EDP_Db_Transaction 호출 (round 2 본체).
    // turn_bucket 산정 정책 D1 = round 2 결재.
    // round 1 = 시그니처 + stub body.
    public class AB_Component_DB_CRUD_Sharding_History : AB_Component_DB_CRUD
    {
        public AB_Component_DB_CRUD_Sharding_History()
        {
        }

        public override Task AddAsync_<T>(T _row) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Sharding_History.AddAsync_: round 2 본체");
        }

        public override Task<T?> GetByIdAsync_<T>(long _id) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Sharding_History.GetByIdAsync_: round 2 본체");
        }

        public override Task<List<T>> FindAsync_<T>(Expression<Func<T, bool>> _predicate) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Sharding_History.FindAsync_: round 2 본체");
        }

        public override Task UpdateAsync_<T>(T _row) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Sharding_History.UpdateAsync_: round 2 본체");
        }

        public override Task RemoveAsync_<T>(T _row) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Sharding_History.RemoveAsync_: round 2 본체");
        }

        public override void Dispose()
        {
        }
    }
}
