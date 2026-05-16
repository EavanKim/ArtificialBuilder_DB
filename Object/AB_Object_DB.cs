using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Component;
using EDPFW;

namespace ArtificialBuilder.DB.Object
{
    // DB 동작 통일 추상 인터페이스. AB_Manager_DB 가 보는 단일 시그니처.
    // canon § "Object / Component 다형성 조립 패턴" 정본 (2026-05-17) — 매니저는 본 추상 만 매개 동작 / 콘크리트 노출 X.
    //
    // 변동 축 2 = Component family 2:
    //   m_file : AB_Component_DB_File   (파일 / 폴더 lifecycle — Normal / Sharding 2 콘크리트)
    //   m_crud : AB_Component_DB_CRUD   (CRUD 라우팅 — Normal / Sharding_Key / Sharding_History 3 콘크리트)
    //
    // 콘크리트 Object (Normal / Sharding_Key / Sharding_History) = ctor 안 콘크리트 Component instance new + abstract field 매개 노출.
    // 본 추상 메서드 본체 = Component 위임 stub (실 동작 = round 2 매개 콘크리트 Component 안 채움).
    //
    // Round 1 (skeleton) 범위 = 시그니처 + stub body 만. CRUD 본체 / 파일 open 본체 = round 2.
    public abstract class AB_Object_DB : AB_Object
    {
        // 변동 축 1 — 파일 lifecycle. ctor 안 콘크리트 instance 매개 채움. 외부 setter X.
        protected AB_Component_DB_File? m_file;

        // 변동 축 2 — CRUD 라우팅. ctor 안 콘크리트 instance 매개 채움. 외부 setter X.
        protected AB_Component_DB_CRUD? m_crud;

        protected AB_Object_DB()
        {
            m_file = null;
            m_crud = null;
        }

        // 파일 / 폴더 lifecycle open. Component 위임.
        // Normal = 파일 1 open / Sharding = 폴더 준비 (실 파일 = 첫 row 매개 lazy).
        public virtual void Open_(string _root_path, EDP_Db_Engine _engine)
        {
            if (m_file == null)
            {
                throw new InvalidOperationException("AB_Object_DB.Open_: m_file 미부착 — 콘크리트 ctor 안 instance 주입 의무");
            }
            m_file.Open_(_root_path, _engine);
        }

        // 보유 handle 전수 close cascade. Component 위임.
        public virtual void Close_()
        {
            if (m_file == null)
            {
                throw new InvalidOperationException("AB_Object_DB.Close_: m_file 미부착");
            }
            m_file.Close_();
        }

        // row 추가. Component 위임 — 콘크리트 CRUD 가 row 매개 핸들 라우팅.
        public virtual Task AddRowAsync_<T>(T _row) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.AddRowAsync_: m_crud 미부착");
            }
            return m_crud.AddAsync_(_row);
        }

        // Id 매개 단일 row 조회. Normal = 단일 handle / Sharding = round 2 라우팅 정책.
        public virtual Task<T?> GetByIdAsync_<T>(long _id) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.GetByIdAsync_: m_crud 미부착");
            }
            return m_crud.GetByIdAsync_<T>(_id);
        }

        // predicate 매개 검색. Sharding = round 2 모든 handle 순회 vs hint 결재.
        public virtual Task<List<T>> FindAsync_<T>(Expression<Func<T, bool>> _predicate) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.FindAsync_: m_crud 미부착");
            }
            return m_crud.FindAsync_<T>(_predicate);
        }

        // row 갱신. Component 위임.
        public virtual Task UpdateAsync_<T>(T _row) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.UpdateAsync_: m_crud 미부착");
            }
            return m_crud.UpdateAsync_(_row);
        }

        // row 삭제. Component 위임.
        public virtual Task RemoveAsync_<T>(T _row) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.RemoveAsync_: m_crud 미부착");
            }
            return m_crud.RemoveAsync_(_row);
        }

        // 보유 handle 전수 dirty flush. File 매개 EDP_Db_Engine 호출.
        public virtual void SyncDirtyToFile_()
        {
            if (m_file == null)
            {
                throw new InvalidOperationException("AB_Object_DB.SyncDirtyToFile_: m_file 미부착");
            }
            m_file.SyncDirtyToFile_();
        }

        // AB_Object abstract 충족 — 본 추상 단계 message handler X. 콘크리트 별 필요 시 override.
        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose()
        {
            if (m_crud != null)
            {
                m_crud.Dispose();
                m_crud = null;
            }
            if (m_file != null)
            {
                m_file.Dispose();
                m_file = null;
            }
        }
    }
}
