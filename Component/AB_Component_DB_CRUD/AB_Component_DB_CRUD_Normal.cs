using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EDPFW;

namespace ArtificialBuilder.DB.Component
{
    // 콘크리트 — 단일 handle 매개 CRUD. 에셋 DB (Normal File). AB_Object_DB_Normal 안 m_crud instance.
    // D5=b — caller 매개 txn 주입 → CRUD = 위임 만 (state X). Sharding family 는 row 매개 handle 라우팅 후 위임 (round 2b).
    public class AB_Component_DB_CRUD_Normal : AB_Component_DB_CRUD
    {
        public AB_Component_DB_CRUD_Normal()
        {
        }

        public override Task AddAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class
        {
            return _txn.AddAsync(_row);
        }

        public override Task<T?> GetByIdAsync_<T>(EDP_Db_Transaction _txn, long _id) where T : class
        {
            return _txn.GetByIdAsync<T>(_id);
        }

        public override Task<List<T>> FindAsync_<T>(EDP_Db_Transaction _txn, Expression<Func<T, bool>> _predicate) where T : class
        {
            return _txn.FindAsync<T>(_predicate);
        }

        public override Task UpdateAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class
        {
            _txn.Update(_row);
            return Task.CompletedTask;
        }

        public override Task RemoveAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class
        {
            _txn.Remove(_row);
            return Task.CompletedTask;
        }

        public override Task<List<T>> FindSimilarAsync_<T>(EDP_Db_Transaction _txn, string _sql, params object[] _params) where T : class
        {
            // Normal = 에셋 DB 매개 vector 사용 X 매개 미지원. caller 매개 Result (Sharding_Key) 매개 Object 매개 호출.
            throw new System.NotSupportedException("AB_Component_DB_CRUD_Normal.FindSimilarAsync_: Normal DB 매개 vector similarity 매개 미지원 — Sharding_Key (Result) 매개 사용");
        }

        public override void Dispose()
        {
        }
    }
}
