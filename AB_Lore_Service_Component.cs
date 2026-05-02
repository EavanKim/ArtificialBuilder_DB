using EDPFW;
using ArtificialBuilder_EDP.Core;
using ArtificialBuilder;

namespace ArtificialBuilder_EDP.Components
{
    /// <summary>AB_Lore_Service 래핑 컴포넌트. DB Proxy 만 사용 — 별도 의존 주입 없음.</summary>
    public class AB_Lore_Service_Component : ArtificialBuilder_EDP.Core.AB_Component
    {
        /// <summary>래핑된 로어 서비스</summary>
        private AB_Lore_Service m_instance = new();
        public AB_Lore_Service Instance
        {
            get { return m_instance; }
            private set { m_instance = value; }
        }

        public override void OnAttach()
        {
            Instance.Initialize();
        }
    }
}
