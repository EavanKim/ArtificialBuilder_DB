using System;
using EDPFW;

namespace ArtificialBuilder.DB.Component
{
    // 콘크리트 — 단일 파일 lifecycle. 에셋 DB (App / Persona / Package) — 전체 로드.
    // canon § "Object / Component 다형성 조립 패턴" 정합. AB_Object_DB_Normal 안 m_file instance 매개 attach.
    //
    // round 1 (skeleton) = field + 시그니처 + stub body 만. Open_ 본체 (EDP_Db_Engine.OpenDatabase 호출 + handle 보관) = round 2.
    public class AB_Component_DB_File_Normal : AB_Component_DB_File
    {
        // EDP_Db_Engine 발급 단일 핸들. 0 = 미부착 (Crash-First).
        // m_engine ref = round 2 본체 진입 시 추가 (Open_ 매개 주입 + SyncDirtyToFile_ / Close_ 시점 사용).
        private int m_handle;

        public AB_Component_DB_File_Normal()
        {
            m_handle = 0;
        }

        public int Handle_ => m_handle;

        // round 2 본체 — EDP_Db_Engine.OpenDatabase<TContext>(_root_path, _factory) 매개 단일 파일 open + handle 보관.
        // 본 round = skeleton stub. 콘크리트 별 DbContext factory 주입 결재 (round 2).
        public override void Open_(string _root_path, EDP_Db_Engine _engine)
        {
            throw new NotImplementedException("AB_Component_DB_File_Normal.Open_: round 2 본체");
        }

        public override void Close_()
        {
            throw new NotImplementedException("AB_Component_DB_File_Normal.Close_: round 2 본체");
        }

        public override void SyncDirtyToFile_()
        {
            throw new NotImplementedException("AB_Component_DB_File_Normal.SyncDirtyToFile_: round 2 본체");
        }

        public override void Dispose()
        {
            m_handle = 0;
        }
    }
}
