using EDPFW;
using ArtificialBuilder_EDP.Core;
using ArtificialBuilder;
using System;
using System.Collections.Generic;

namespace ArtificialBuilder_EDP.Components
{
    /// <summary>AB_DB 래핑 컴포넌트. App/Persona/Circuit 3개 서브 DB 접근 제공.</summary>
    public class AB_Db_Component : ArtificialBuilder_EDP.Core.AB_Component
    {
        /// <summary>래핑된 DB 인스턴스</summary>
        private AB_DB m_instance = new();
        public AB_DB Instance
        {
            get { return m_instance; }
            private set { m_instance = value; }
        }

        /// <summary>전역 앱 DB (모델 설정, UI 템플릿, 파이프라인)</summary>
        public AB_App_Db App => Instance.App;
        /// <summary>페르소나 DB (사용자별 채팅 세션)</summary>
        public AB_Persona_Db Persona => Instance.Persona;
        /// <summary>Circuit DB (현재 열린 Circuit의 설정/에셋)</summary>
        public AB_Circuit_Db Circuit => Instance.Circuit;

        /// <summary>DB는 Program.Main에서 종료 처리하므로 비움</summary>
        public override void OnDetach() { /* disposal handled by Program.Main */ }

        /// <summary>단일 스레드 dirty 엔트리 flush. Tick 단계 — 경쟁 없음. [[blackboard-db]].</summary>
        public override IEnumerator<EDP_Coroutine_State> Tick(double _deltaSec)
        {
            try
            {
                Instance.SyncDirtyToFile();
            }
            catch (Exception ex)
            {
                AB_Log.Critical("DbComponent", $"DB flush 실패: {ex.Message}");
            }
            yield break;
        }
    }
}
