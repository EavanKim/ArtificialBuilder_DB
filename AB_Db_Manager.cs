using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Core;
using EDPFW;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 단일 DB 매니저 ([[app-logic-separation]] 5 도메인 + [[unify-entry-points]] + [[blackboard-db]]).
    /// 5 도메인 DB (App / Persona / Circuit / Logic / ResponseUi) lifecycle + dirty flush + 도메인 Proxy 진입.
    /// 외부 호출 = DDO publish (AB_DDO_Headers.{APP,PERSONA,CIRCUIT,LOGIC,RESPONSE_UI}_DB_*) 또는 본 Manager 의 도메인별 Proxy 헬퍼.
    /// </summary>
    public class AB_Db_Manager : AB_Component
    {
        /// <summary>5 도메인 DB lifecycle 본체.</summary>
        private AB_DB m_instance = new();
        public AB_DB Instance
        {
            get { return m_instance; }
            private set { m_instance = value; }
        }

        // --- 도메인 lifecycle 단축 ---

        /// <summary>App DB (글로벌, 로직 무관 [[app-logic-separation]]).</summary>
        public AB_App_Db App => Instance.App;
        /// <summary>Persona DB (별도 schema 보존).</summary>
        public AB_Persona_Db Persona => Instance.Persona;
        /// <summary>Circuit DB (사용자 완성품 / 노드 그래프 + 자원).</summary>
        public AB_Circuit_Db Circuit => Instance.Circuit;
        /// <summary>Logic DB (per-logic, 노드 정보).</summary>
        public AB_Logic_Db Logic => Instance.Logic;
        /// <summary>Response UI DB (per-response-ui, 화면 구성).</summary>
        public AB_Response_Ui_Db ResponseUi => Instance.ResponseUi;

        // --- 도메인 Proxy 단축 (read/write 진입 — sub 5 에서 DDO 옵저버로 보강) ---

        /// <summary>App DB Proxy ([[ddo-command-only]] sub 5 에서 DDO 옵저버 추가).</summary>
        public AB_App_Db_Proxy AppProxy => global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>();
        /// <summary>Persona DB Proxy.</summary>
        public AB_Persona_Db_Proxy PersonaProxy => global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Persona_Db_Proxy>();
        /// <summary>Circuit DB Proxy.</summary>
        public AB_Circuit_Db_Proxy CircuitProxy => global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>();
        /// <summary>Logic DB Proxy.</summary>
        public AB_Logic_Db_Proxy LogicProxy => global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>();
        /// <summary>Response UI DB Proxy.</summary>
        public AB_Response_Ui_Db_Proxy ResponseUiProxy => global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Response_Ui_Db_Proxy>();

        /// <summary>5 도메인 DB lifecycle 초기화 + 활성 페르소나 로드.</summary>
        public async Task InitializeAsync()
        {
            await Instance.InitializeAsync();
        }

        /// <summary>DB 는 Program.Main 에서 종료 처리하므로 비움.</summary>
        public override void OnDetach() { /* disposal handled by Program.Main */ }

        /// <summary>단일 스레드 dirty 엔트리 flush. Tick 단계 — 경쟁 없음. [[blackboard-db]].</summary>
        public override IEnumerator<EDP_Coroutine_State> run(double _deltaSec)
        {
            try
            {
                Instance.SyncDirtyToFile();
            }
            catch (Exception ex)
            {
                AB_Log.Critical("DbManager", $"DB flush 실패: {ex.Message}");
            }
            yield break;
        }
    }
}
