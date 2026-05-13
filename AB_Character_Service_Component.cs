using System;
using System.Collections.Generic;
using EDPFW;
using ArtificialBuilder_EDP.Core;
using ArtificialBuilder;
using ArtificialBuilder.DDO;
using ArtificialBuilder.Models;

namespace ArtificialBuilder_EDP.Components
{
    /// <summary>AB_Character_Service 래핑 컴포넌트. DB Proxy 만 사용 — 별도 의존 주입 없음.
    /// (windows-ddo-migration sub 2) UI 측 직접 호출 X — DDO 옵저버 등록 + handler 가 본체 Instance 위임.</summary>
    public class AB_Character_Service_Component : ArtificialBuilder_EDP.Core.AB_Component
    {
        /// <summary>래핑된 캐릭터 서비스</summary>
        private AB_Character_Service m_instance = new();
        public AB_Character_Service Instance
        {
            get { return m_instance; }
            private set { m_instance = value; }
        }

        public override void OnAttach()
        {
            Instance.Initialize();
            AB_DDO_Subscription_Manager mgr = AB_Engine.GetService<AB_DDO_Subscription_Manager>();
            mgr.AddObserverFor(this, AB_Object_Command_Type.CHARACTER_REFRESH, HandleRefresh);
            mgr.AddObserverFor(this, AB_Object_Command_Type.CHARACTER_GET_ALL, HandleGetAll);
            mgr.AddObserverFor(this, AB_Object_Command_Type.CHARACTER_GET_RELATIONSHIPS, HandleGetRelationships);
            mgr.AddObserverFor(this, AB_Object_Command_Type.CHARACTER_GET_LOCATIONS, HandleGetLocations);
            mgr.AddObserverFor(this, AB_Object_Command_Type.CHARACTER_GET_CONNECTIONS, HandleGetConnections);
        }

        public override void OnDetach()
        {
            AB_Engine.GetService<AB_DDO_Subscription_Manager>().UnregisterOwner(this);
        }

        private static void PublishResult(AB_Object_Command_Type _header, AB_Id? _queryId, object _result)
        {
            if (_queryId == null) return;
            if (!AB_Engine.TryGet<AB_DDO_Subscription_Manager>(out var ddo)) return;
            ddo.Publish(_header, null, _result, _toId: _queryId);
        }

        private void HandleRefresh(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            async void RunAsync()
            {
                await m_instance.RefreshAsync();
                PublishResult(AB_Object_Command_Type.CHARACTER_REFRESH, queryId, new AB_Character_Refresh_Result { Ok_ = true });
            }
            RunAsync();
        }

        private void HandleGetAll(AB_DDO_Command _cmd)
        {
            PublishResult(AB_Object_Command_Type.CHARACTER_GET_ALL, _cmd.FromId,
                new AB_Character_Get_All_Result { Characters_ = new List<AB_Character_Model>(m_instance.Items) });
        }

        private void HandleGetRelationships(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            async void RunAsync()
            {
                List<AB_Character_Relationship_Model> rels = await m_instance.GetAllRelationshipsAsync();
                PublishResult(AB_Object_Command_Type.CHARACTER_GET_RELATIONSHIPS, queryId,
                    new AB_Character_Get_Relationships_Result { Relationships_ = rels });
            }
            RunAsync();
        }

        private void HandleGetLocations(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            async void RunAsync()
            {
                List<AB_Location_Model> locs = await m_instance.GetAllLocationsAsync();
                PublishResult(AB_Object_Command_Type.CHARACTER_GET_LOCATIONS, queryId,
                    new AB_Character_Get_Locations_Result { Locations_ = locs });
            }
            RunAsync();
        }

        private void HandleGetConnections(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            async void RunAsync()
            {
                List<AB_Location_Connection_Model> conns = await m_instance.GetAllLocationConnectionsAsync();
                PublishResult(AB_Object_Command_Type.CHARACTER_GET_CONNECTIONS, queryId,
                    new AB_Character_Get_Connections_Result { Connections_ = conns });
            }
            RunAsync();
        }
    }
}
