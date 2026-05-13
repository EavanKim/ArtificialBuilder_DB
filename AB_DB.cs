using EDPFW;
using System;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>App/Persona/Circuit/Logic/ResponseUi 5개 서브 DB를 묶는 루트 DB ([[app-logic-separation]] 5 도메인).</summary>
    // TODO[canon-conformance phase-6]: (E) EDP_Db_Engine 직접 후손 = AB_Object 우회.
    // TODO[canon-conformance phase-4]: (D) AB_App_Db / Persona / Circuit / Logic / ResponseUi 5 holder = Component ref 직접 보유. DI lookup property 마이그 필수.
    // 정본: docs/plans/todo/canon-conformance/phase-6-edp-direct-inherit-bridge.md / phase-4-component-ref-cleanup.md
    public class AB_DB : EDP_Db_Engine
    {
        // --- 초기화 ---

        /// <summary>5 도메인 DB 초기화 + 활성 페르소나 로드 ([[app-logic-separation]]).</summary>
        public async Task InitializeAsync()
        {
            App = new AB_App_Db();
            App.Initialize(this);

            Persona = new AB_Persona_Db();
            Persona.Initialize(this);

            Circuit = new AB_Circuit_Db();
            Circuit.Initialize(this);

            Logic = new AB_Logic_Db();
            Logic.Initialize(this);

            ResponseUi = new AB_Response_Ui_Db();
            ResponseUi.Initialize(this);

            // Timer Sync 제거 — 칠판 Tick에서 SyncDirtyToFile() 실행
            await Persona.LoadActiveAsync();
        }

        // --- DB 노드 ---

        /// <summary>전역 앱 DB (모델 설정/UI 템플릿/파이프라인). 로직과 무관 ([[app-logic-separation]]).</summary>
        private AB_App_Db m_app = null!;
        public AB_App_Db App
        {
            get { return m_app; }
            private set { m_app = value; }
        }
        /// <summary>활성 페르소나 DB (별도 schema 보존 — chat 메시지 = 로직 히스토리 동의어).</summary>
        private AB_Persona_Db m_persona = null!;
        public AB_Persona_Db Persona
        {
            get { return m_persona; }
            private set { m_persona = value; }
        }
        /// <summary>활성 Circuit DB (사용자 완성품 — 노드 그래프 + 자원).</summary>
        private AB_Circuit_Db m_circuit = null!;
        public AB_Circuit_Db Circuit
        {
            get { return m_circuit; }
            private set { m_circuit = value; }
        }
        /// <summary>활성 Logic DB (per-logic — 노드 정보만 / 사용 서킷+모델 키+ResponseUI 키+sub-logic+history).</summary>
        private AB_Logic_Db m_logic = null!;
        public AB_Logic_Db Logic
        {
            get { return m_logic; }
            private set { m_logic = value; }
        }
        /// <summary>활성 Response UI DB (per-response-ui — 화면 구성 / Window+Component+Layer+Template).</summary>
        private AB_Response_Ui_Db m_responseUi = null!;
        public AB_Response_Ui_Db ResponseUi
        {
            get { return m_responseUi; }
            private set { m_responseUi = value; }
        }
    }
}
