using EDPFW;
using System;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>App/Persona/Circuit 3개 서브 DB를 묶는 루트 DB.</summary>
    public class AB_DB : EDP_Db_Engine
    {
        // --- 초기화 ---

        /// <summary>3개 서브 DB 초기화 + 30초 주기 동기화 + 활성 페르소나 로드.</summary>
        public async Task InitializeAsync()
        {
            App = new AB_App_Db();
            App.Initialize(this);

            Persona = new AB_Persona_Db();
            Persona.Initialize(this);

            Circuit = new AB_Circuit_Db();
            Circuit.Initialize(this);

            // Timer Sync 제거 — 칠판 Tick에서 SyncDirtyToFile() 실행
            await Persona.LoadActiveAsync();
        }

        // --- DB 노드 ---

        /// <summary>전역 앱 DB (모델 설정/UI 템플릿/파이프라인).</summary>
        private AB_App_Db m_app = null!;
        public AB_App_Db App
        {
            get { return m_app; }
            private set { m_app = value; }
        }
        /// <summary>활성 페르소나 DB (채팅 세션/메시지).</summary>
        private AB_Persona_Db m_persona = null!;
        public AB_Persona_Db Persona
        {
            get { return m_persona; }
            private set { m_persona = value; }
        }
        /// <summary>활성 Circuit DB (Circuit 설정/에셋/캐릭터).</summary>
        private AB_Circuit_Db m_circuit = null!;
        public AB_Circuit_Db Circuit
        {
            get { return m_circuit; }
            private set { m_circuit = value; }
        }
    }
}
