using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Component
{
    // 변동 축 2 — CRUD 라우팅 family abstract.
    // canon § "Object / Component 다형성 조립 패턴" 정합. AB_Object_DB 안 m_crud abstract field 매개 보유.
    // 콘크리트 3:
    //   _Normal           = 단일 handle 매개 CRUD (에셋 DB)
    //   _Sharding_Key     = 키 매개 핸들 라우팅 (Node 페이로드)
    //   _Sharding_History = 시계열 매개 핸들 라우팅 (Turn 히스토리)
    //
    // 본 abstract = 시그니처 만. row 매개 핸들 라우팅 정책 = 콘크리트 별 캡슐화 (round 2).
    public abstract class AB_Component_DB_CRUD : AB_Component
    {
        public abstract Task AddAsync_<T>(T _row) where T : class;
        public abstract Task<T?> GetByIdAsync_<T>(long _id) where T : class;
        public abstract Task<List<T>> FindAsync_<T>(Expression<Func<T, bool>> _predicate) where T : class;
        public abstract Task UpdateAsync_<T>(T _row) where T : class;
        public abstract Task RemoveAsync_<T>(T _row) where T : class;
    }
}
