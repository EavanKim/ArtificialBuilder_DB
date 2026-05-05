using ArtificialBuilder;
using ArtificialBuilder_EDP.Components;

namespace ArtificialBuilder_EDP
{
    /// <summary>
    /// 칠판 데이터 관리자 static facade. 모든 데이터 접근의 단일 진입점.
    /// Core.AB_Engine.Get&lt;AB_Db_Manager&gt;().Instance.Persona → AB_Board.Persona
    /// </summary>
    public static class AB_Board
    {
        private static AB_DB? g_db;
        private static Core.AB_Blackboard? g_blackboard;

        /// <summary>초기화. Program.cs에서 한 번 호출.</summary>
        public static void Bind(AB_DB _db, Core.AB_Blackboard _blackboard)
        {
            g_db = _db;
            g_blackboard = _blackboard;
        }

        /// <summary>칠판 컴포넌트 (슬롯/이벤트).</summary>
        public static Core.AB_Blackboard Blackboard => g_blackboard!;

        /// <summary>DB 엔진 (직접 접근은 최소화).</summary>
        public static AB_DB Db => g_db!;

        /// <summary>페르소나 DB.</summary>
        public static AB_Persona_Db Persona => g_db!.Persona;

        /// <summary>Circuit DB.</summary>
        public static AB_Circuit_Db Circuit => g_db!.Circuit;

        /// <summary>앱 DB.</summary>
        public static AB_App_Db App => g_db!.App;

        /// <summary>Logic DB ([[app-logic-separation]] — 노드 정보만).</summary>
        public static AB_Logic_Db Logic => g_db!.Logic;

        /// <summary>Response UI DB ([[app-logic-separation]] — 화면 구성).</summary>
        public static AB_Response_Ui_Db ResponseUi => g_db!.ResponseUi;
    }
}
