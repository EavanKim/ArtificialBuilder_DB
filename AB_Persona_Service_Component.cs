using EDPFW;
using ArtificialBuilder_EDP.Core;
using ArtificialBuilder;

namespace ArtificialBuilder_EDP.Components
{
    /// <summary>AB_Persona_Service 래핑 컴포넌트. AB_Board.Persona 만 사용 — 별도 의존 주입 없음.</summary>
    public class AB_Persona_Service_Component : ArtificialBuilder_EDP.Core.AB_Component
    {
        /// <summary>래핑된 페르소나 서비스</summary>
        private AB_Persona_Service m_instance = new();
        public AB_Persona_Service Instance
        {
            get { return m_instance; }
            private set { m_instance = value; }
        }

        /// <summary>EDP_Service_Engine.Initialize() 만 호출.</summary>
        public override void OnAttach()
        {
            Instance.Initialize();
        }
    }
}
