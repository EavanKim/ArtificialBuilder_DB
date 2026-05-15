using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;
using EDPFW;

namespace ArtificialBuilder.DB
{
    // 모듈 최상위 매니저. AB_Manager (root 4) 후손 — 별도 생존주기 / 풀 X / Loop 등록 X.
    // EDP_Db_Engine 단일 소유. 프로그램 단위 lifetime — 5 도메인 (App/Persona/Circuit/Logic/Response_Ui) 매개 단일 engine 공유.
    // 5 도메인 (App/Persona/Circuit/Logic/Response_Ui) AB_Object_Db_* 단일 진입 lookup. 본체 lifecycle = 각 도메인 Object 매개 의무.
    // 외부 호출 = DDO publish 매개 중앙 처리기 dispatch. 매니저 = lookup + engine 진입점 만.
    public class AB_Db_Manager : AB_Manager
    {
        private readonly EDP_Db_Engine m_engine;

        public AB_Db_Manager()
        {
            m_engine = new EDP_Db_Engine();
        }

        public EDP_Db_Engine Engine => m_engine;

        public AB_Object_Db_App App => EDP_Container.Get<AB_Object_Db_App>();
        public AB_Object_Db_Persona Persona => EDP_Container.Get<AB_Object_Db_Persona>();
        public AB_Object_Db_Circuit Circuit => EDP_Container.Get<AB_Object_Db_Circuit>();
        public AB_Object_Db_Logic Logic => EDP_Container.Get<AB_Object_Db_Logic>();
        public AB_Object_Db_Response_Ui ResponseUi => EDP_Container.Get<AB_Object_Db_Response_Ui>();

        // Loop tick 매개 호출. 매 tick 단일 직렬화 (engine 안 _syncLock) 매개 dirty entry 파일 flush.
        public void SyncDirtyToFile()
        {
            m_engine.SyncDirtyToFile();
        }

        public override void Dispose()
        {
        }
    }
}
