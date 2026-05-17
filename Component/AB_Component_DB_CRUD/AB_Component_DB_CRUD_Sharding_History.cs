using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EDPFW;

namespace ArtificialBuilder.DB.Component
{
    // 콘크리트 — Turn 시계열 샤딩 CRUD. AB_Object_DB_Sharding_History 안 m_crud instance.
    // shard_key (= turn_id / 100) 매개 handle 라우팅 = Object 매개 BeginTransactionAsync_(shard_key) 시점 결정 — 본 Component 는 txn 직접 위임 (state X).
    // D5=b — caller 매개 txn 주입 → CRUD = 위임 만. CRUD_Normal 패턴 1:1.
    // FK 최저화 룰 매개 cross-shard 조인 / 무결성 = 어플리케이션 (multi-shard 작업 = caller 매개 shard_key 별 별도 txn).
    public class AB_Component_DB_CRUD_Sharding_History : AB_Component_DB_CRUD
    {
        public AB_Component_DB_CRUD_Sharding_History()
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
            // Sharding_History = Turn 시계열 매개 vector 사용 X 매개 미지원. caller 매개 Result (Sharding_Key) 매개 Object 매개 호출.
            throw new System.NotSupportedException("AB_Component_DB_CRUD_Sharding_History.FindSimilarAsync_: Turn 시계열 매개 vector similarity 매개 미지원 — Sharding_Key (Result) 매개 사용");
        }

        public override void Dispose()
        {
        }
    }
}
