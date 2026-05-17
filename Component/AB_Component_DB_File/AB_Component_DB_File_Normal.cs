using System;
using System.Threading.Tasks;
using EDPFW;

namespace ArtificialBuilder.DB.Component
{
    // 콘크리트 — 단일 파일 lifecycle. 에셋 DB (App / Persona / Package) — 전체 로드.
    // canon § "Object / Component 다형성 조립 패턴" 정합. AB_Object_DB_Normal 안 m_file instance 매개 attach.
    public class AB_Component_DB_File_Normal : AB_Component_DB_File
    {
        // EDP_Db_Engine 발급 단일 핸들. 0 = 미부착 (Crash-First).
        private int m_handle;

        // Open_ 매개 주입 engine ref — Close_ / BeginTransactionAsync_ / SyncDirtyToFile_ 시점 사용.
        private EDP_Db_Engine? m_engine;

        public AB_Component_DB_File_Normal()
        {
            m_handle = 0;
            m_engine = null;
        }

        public int Handle_ => m_handle;

        public override void Open_(string _root_path, EDP_Db_Engine _engine, Func<EDP_Db_Engine, string, int> _opener)
        {
            if (m_handle != 0)
            {
                throw new InvalidOperationException("AB_Component_DB_File_Normal.Open_: 이미 open 상태");
            }
            m_engine = _engine;
            m_handle = _opener(_engine, _root_path);
            if (m_handle == 0)
            {
                throw new InvalidOperationException("AB_Component_DB_File_Normal.Open_: 핸들 발급 실패 (DbSet 검증 실패 / opener 0 반환)");
            }
        }

        public override void Close_()
        {
            if (m_handle == 0)
            {
                return;
            }
            if (m_engine == null)
            {
                throw new InvalidOperationException("AB_Component_DB_File_Normal.Close_: m_engine 미부착 (Open_ 미경유)");
            }
            m_engine.CloseAsync(m_handle).AsTask().GetAwaiter().GetResult();
            m_handle = 0;
            m_engine = null;
        }

        public override Task<EDP_Db_Transaction> BeginTransactionAsync_()
        {
            if (m_handle == 0)
            {
                throw new InvalidOperationException("AB_Component_DB_File_Normal.BeginTransactionAsync_: 미 open (m_handle == 0)");
            }
            if (m_engine == null)
            {
                throw new InvalidOperationException("AB_Component_DB_File_Normal.BeginTransactionAsync_: m_engine 미부착");
            }
            return m_engine.BeginTransactionAsync(m_handle);
        }

        // Normal = 단일 handle. shard_key overload 사용 X.
        public override Task<EDP_Db_Transaction> BeginTransactionAsync_(long _shard_key)
        {
            throw new NotSupportedException("AB_Component_DB_File_Normal.BeginTransactionAsync_(long): Normal = 단일 파일 — no-arg overload 사용");
        }

        public override void SyncDirtyToFile_()
        {
            if (m_engine == null)
            {
                return;
            }
            m_engine.SyncDirtyToFile();
        }

        public override void Dispose()
        {
            Close_();
        }
    }
}
