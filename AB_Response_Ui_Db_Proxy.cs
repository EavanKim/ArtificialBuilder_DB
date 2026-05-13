using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtificialBuilder_EDP.Core;

// 주의: Window / Component / Template 관련 Proxy 메서드는 OLD AB_Circuit_Db_Proxy 에 보존 (Topic 만 ActiveResponseUi 로 redirect). 본 Proxy = Meta + Layer 신설 영역.
namespace ArtificialBuilder
{
    /// <summary>Response UI DB 외부 진입 프록시 (Meta + Layer 신설 영역).</summary>
    public class AB_Response_Ui_Db_Proxy : ArtificialBuilder_EDP.Core.AB_Object
    {
        private TimeSpan m_defaultTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan DefaultTimeout
        {
            get { return m_defaultTimeout; }
            set { m_defaultTimeout = value; }
        }

        private IAB_Message_Broker GetBroker()
            => ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();

        // --- Meta ---

        public async Task<AB_Response_Ui_Meta_Model?> GetMetaAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Response_Ui_Meta_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Response_Ui_Meta_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        public async Task<bool> SaveMetaAsync(AB_Response_Ui_Meta_Model _meta)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Response_Ui_Meta_Request>();
            req.Meta = _meta;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Response_Ui_Meta_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // --- Layers ---

        public async Task<List<AB_Response_Ui_Layer_Model>> GetLayersAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Response_Ui_Layers_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Response_Ui_Layers_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddLayerAsync(AB_Response_Ui_Layer_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Response_Ui_Layer_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Response_Ui_Layer_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> SaveLayerAsync(AB_Response_Ui_Layer_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Response_Ui_Layer_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Response_Ui_Layer_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> DeleteLayerAsync(AB_Response_Ui_Layer_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Response_Ui_Layer_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Response_Ui_Layer_Response>(req, DefaultTimeout);
            return resp.Success;
        }
    }
}
