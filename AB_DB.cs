using EDPFW;
using ArtificialBuilder_EDP.Core;
using System;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>App/Persona/Circuit/Logic/ResponseUi 5개 서브 DB를 묶는 루트 DB ([[app-logic-separation]] 5 도메인).</summary>
    // TODO[canon-conformance phase-6]: (E) EDP_Db_Engine 직접 후손 = AB_Object 우회.
    public class AB_DB : EDP_Db_Engine
    {
        // --- 초기화 ---

        /// <summary>5 도메인 DB 초기화 + 활성 페르소나 로드 ([[app-logic-separation]]).</summary>
        public async Task InitializeAsync()
        {
            // (canon-conformance phase-4 a, 2026-05-13) sub_DB 5 종 = DI Container 가 lifecycle 관리. holder 폐기 + lookup property.
            // 본 InitializeAsync 는 host this 매개 Initialize 호출만 책임.
            App.Initialize(this);
            Persona.Initialize(this);
            Circuit.Initialize(this);
            Logic.Initialize(this);
            ResponseUi.Initialize(this);

            // Timer Sync 제거 — 칠판 Tick에서 SyncDirtyToFile() 실행
            await Persona.LoadActiveAsync();
        }

        // --- DB 노드 (DI lookup property — canon-conformance phase-4 a) ---

        /// <summary>전역 앱 DB (모델 설정/UI 템플릿/파이프라인). 로직과 무관 ([[app-logic-separation]]).</summary>
        public AB_App_Db App => AB_Engine.GetService<AB_App_Db>();

        /// <summary>활성 페르소나 DB (별도 schema 보존 — chat 메시지 = 로직 히스토리 동의어).</summary>
        public AB_Persona_Db Persona => AB_Engine.GetService<AB_Persona_Db>();

        /// <summary>활성 Circuit DB (사용자 완성품 — 노드 그래프 + 자원).</summary>
        public AB_Circuit_Db Circuit => AB_Engine.GetService<AB_Circuit_Db>();

        /// <summary>활성 Logic DB (per-logic — 노드 정보만 / 사용 서킷+모델 키+ResponseUI 키+sub-logic+history).</summary>
        public AB_Logic_Db Logic => AB_Engine.GetService<AB_Logic_Db>();

        /// <summary>활성 Response UI DB (per-response-ui — 화면 구성 / Window+Component+Layer+Template).</summary>
        public AB_Response_Ui_Db ResponseUi => AB_Engine.GetService<AB_Response_Ui_Db>();
    }
}
