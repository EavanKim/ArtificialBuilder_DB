using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArtificialBuilder.DB.Component
{
    // 콘크리트 — 단일 handle 매개 CRUD. 에셋 DB (Normal File). AB_Object_DB_Normal 안 m_crud instance.
    // 본 Component = AB_Component_DB_File_Normal 매개 handle 받아 EDP_Db_Transaction 호출 (round 2 본체).
    // round 1 = 시그니처 + stub body.
    public class AB_Component_DB_CRUD_Normal : AB_Component_DB_CRUD
    {
        public AB_Component_DB_CRUD_Normal()
        {
        }

        public override Task AddAsync_<T>(T _row) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Normal.AddAsync_: round 2 본체");
        }

        public override Task<T?> GetByIdAsync_<T>(long _id) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Normal.GetByIdAsync_: round 2 본체");
        }

        public override Task<List<T>> FindAsync_<T>(Expression<Func<T, bool>> _predicate) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Normal.FindAsync_: round 2 본체");
        }

        public override Task UpdateAsync_<T>(T _row) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Normal.UpdateAsync_: round 2 본체");
        }

        public override Task RemoveAsync_<T>(T _row) where T : class
        {
            throw new NotImplementedException("AB_Component_DB_CRUD_Normal.RemoveAsync_: round 2 본체");
        }

        public override void Dispose()
        {
        }
    }
}
