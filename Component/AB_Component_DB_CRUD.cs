using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArtificialBuilder.Common.Base;
using EDPFW;

namespace ArtificialBuilder.DB.Component
{
    // 변동 축 2 — CRUD 라우팅 family abstract.
    // canon § "Object / Component 다형성 조립 패턴" 정합. AB_Object_DB 안 m_crud abstract field 매개 보유.
    // 콘크리트 3:
    //   _Normal           = 단일 handle 매개 CRUD (에셋 DB) — txn 직접 위임
    //   _Sharding_Key     = 키 매개 핸들 라우팅 (Node 페이로드) — round 2b
    //   _Sharding_History = 시계열 매개 핸들 라우팅 (Turn 히스토리) — round 2b
    //
    // D5=b — txn 인자 매개 caller batch 제어. CRUD = txn 위임 (Normal) 또는 row 매개 handle 라우팅 후 위임 (Sharding round 2b).
    public abstract class AB_Component_DB_CRUD : AB_Component
    {
        public abstract Task AddAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class;
        public abstract Task<T?> GetByIdAsync_<T>(EDP_Db_Transaction _txn, long _id) where T : class;
        public abstract Task<List<T>> FindAsync_<T>(EDP_Db_Transaction _txn, Expression<Func<T, bool>> _predicate) where T : class;
        public abstract Task UpdateAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class;
        public abstract Task RemoveAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class;
    }
}
