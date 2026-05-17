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
    // D6=A — m_opener (opaque opener delegate) 매개 TContext 결정 위치 콘크리트 Object 이양. File 추상 = non-generic 유지.
    // D5=b — caller batch 제어 매개 EDP_Db_Transaction 인자 전달.
    //
    // 콘크리트 Object (Normal / Sharding_Key / Sharding_History) = ctor 안:
    //   1) m_file / m_crud 콘크리트 instance new
    //   2) m_opener = `(engine, path) => engine.OpenDatabase<TContext>(path, factory)` 주입
    public abstract class AB_Object_DB : AB_Object
    {
        protected AB_Component_DB_File? m_file;
        protected AB_Component_DB_CRUD? m_crud;

        // D6=A — 콘크리트 ctor 안 TContext 결정 매개 주입. AB_Object_DB.Open_ 매개 File 에 전달.
        protected Func<EDP_Db_Engine, string, int>? m_opener;

        protected AB_Object_DB()
        {
            m_file = null;
            m_crud = null;
            m_opener = null;
        }

        public virtual void Open_(string _root_path, EDP_Db_Engine _engine)
        {
            if (m_file == null)
            {
                throw new InvalidOperationException("AB_Object_DB.Open_: m_file 미부착 — 콘크리트 ctor 안 instance 주입 의무");
            }
            if (m_opener == null)
            {
                throw new InvalidOperationException("AB_Object_DB.Open_: m_opener 미주입 — 콘크리트 ctor 안 TContext 결정 의무");
            }
            m_file.Open_(_root_path, _engine, m_opener);
        }

        public virtual void Close_()
        {
            if (m_file == null)
            {
                throw new InvalidOperationException("AB_Object_DB.Close_: m_file 미부착");
            }
            m_file.Close_();
        }

        // D5=b — caller 매개 batch 제어. 본 메서드 매개 발급된 txn 을 후속 CRUD 호출 site 매개 전달.
        // Normal 콘크리트 = 단일 파일 매개 동작. Sharding 콘크리트 = NotSupportedException throw — shard_key overload 사용.
        public virtual Task<EDP_Db_Transaction> BeginTransactionAsync_()
        {
            if (m_file == null)
            {
                throw new InvalidOperationException("AB_Object_DB.BeginTransactionAsync_: m_file 미부착");
            }
            return m_file.BeginTransactionAsync_();
        }

        // Sharding family 전용 — shard_key 매개 단일 shard 파일 안 batch 보장. Normal = NotSupportedException throw (file 콘크리트 매개).
        // FK 최저화 룰 매개 cross-shard 무결성 = 어플리케이션 책임 — caller 매개 multi-shard 작업 = shard_key 별 별도 txn.
        public virtual Task<EDP_Db_Transaction> BeginTransactionAsync_(long _shard_key)
        {
            if (m_file == null)
            {
                throw new InvalidOperationException("AB_Object_DB.BeginTransactionAsync_(long): m_file 미부착");
            }
            return m_file.BeginTransactionAsync_(_shard_key);
        }

        public virtual Task AddRowAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.AddRowAsync_: m_crud 미부착");
            }
            return m_crud.AddAsync_(_txn, _row);
        }

        public virtual Task<T?> GetByIdAsync_<T>(EDP_Db_Transaction _txn, long _id) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.GetByIdAsync_: m_crud 미부착");
            }
            return m_crud.GetByIdAsync_<T>(_txn, _id);
        }

        public virtual Task<List<T>> FindAsync_<T>(EDP_Db_Transaction _txn, Expression<Func<T, bool>> _predicate) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.FindAsync_: m_crud 미부착");
            }
            return m_crud.FindAsync_<T>(_txn, _predicate);
        }

        public virtual Task UpdateAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.UpdateAsync_: m_crud 미부착");
            }
            return m_crud.UpdateAsync_(_txn, _row);
        }

        public virtual Task RemoveAsync_<T>(EDP_Db_Transaction _txn, T _row) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.RemoveAsync_: m_crud 미부착");
            }
            return m_crud.RemoveAsync_(_txn, _row);
        }

        // 13r4b 매개 raw SQL 매개 similarity 매개 — Sharding_Key (Result) 매개만 본체. Normal / Sharding_History 매개 NotSupported.
        public virtual Task<List<T>> FindSimilarAsync_<T>(EDP_Db_Transaction _txn, string _sql, params object[] _params) where T : class
        {
            if (m_crud == null)
            {
                throw new InvalidOperationException("AB_Object_DB.FindSimilarAsync_: m_crud 미부착");
            }
            return m_crud.FindSimilarAsync_<T>(_txn, _sql, _params);
        }

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
            m_opener = null;
        }
    }
}
