using System.Collections.Generic;
using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // 앱 도메인 DB (app.db). Db_Read / Db_Write / Db_Delete Component attach.
    public class AB_Object_Db_App : AB_Object_Db
    {
        private readonly List<AB_Component> m_components;

        public AB_Object_Db_App()
        {
            m_components = new List<AB_Component>();
        }

        public void AddComponent(AB_Component _c)
        {
            m_components.Add(_c);
        }

        public override void Tick(double _delta_sec)
        {
            foreach (AB_Component c in m_components)
            {
                c.Tick(_delta_sec);
            }
        }

        // 메시지 수신 entry. 본 클래스 매개 메시지 처리 X — 빈 override 매개 abstract 의무 충족.
        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose()
        {
            foreach (AB_Component c in m_components)
            {
                c.Dispose();
            }
            m_components.Clear();
        }
    }
}
