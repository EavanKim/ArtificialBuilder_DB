using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtificialBuilder_EDP.Core;

namespace ArtificialBuilder
{
    /// <summary>
    /// Logic DB 외부 진입 프록시 (얇은 wrapper). 메시지 브로커 + AB_Logic_Db_Gateway 경유 호출.
    /// 사용:
    ///   var proxy = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>();
    ///   var meta = await proxy.GetMetaAsync();
    /// </summary>
    public class AB_Logic_Db_Proxy : ArtificialBuilder_EDP.Core.AB_Object
    {
        private TimeSpan m_defaultTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan DefaultTimeout
        {
            get { return m_defaultTimeout; }
            set { m_defaultTimeout = value; }
        }

        private IAB_Message_Broker GetBroker()
            => ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();

        // --- 로직 라이브러리 (main-tabs-and-package-system sub 2) ---

        public async Task<long> CreateLogicAsync(string? _name = null, long _uuid = 0L)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Create_Logic_Request>();
            req.Uuid = _uuid;
            req.Name = _name;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Create_Logic_Response>(req, DefaultTimeout);
            return resp.Uuid;
        }

        public async Task<bool> DeleteLogicAsync(long _uuid)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Logic_Request>();
            req.Uuid = _uuid;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Logic_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<List<AB_Logic_Library_Item>> GetLogicLibraryInfoAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Logic_Library_Info_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Logic_Library_Info_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> OpenLogicAsync(long _uuid)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Open_Logic_Request>();
            req.Uuid = _uuid;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Open_Logic_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // --- Meta ---

        public async Task<AB_Logic_Meta_Model?> GetMetaAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Logic_Meta_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Logic_Meta_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        public async Task<bool> SaveMetaAsync(AB_Logic_Meta_Model _meta)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Logic_Meta_Request>();
            req.Meta = _meta;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Logic_Meta_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // --- Used Circuits ---

        public async Task<List<AB_Logic_Used_Circuit_Model>> GetUsedCircuitsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Logic_Used_Circuits_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Used_Circuits_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddUsedCircuitAsync(AB_Logic_Used_Circuit_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Logic_Used_Circuit_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Used_Circuit_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> SaveUsedCircuitAsync(AB_Logic_Used_Circuit_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Logic_Used_Circuit_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Logic_Used_Circuit_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> DeleteUsedCircuitAsync(AB_Logic_Used_Circuit_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Logic_Used_Circuit_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Logic_Used_Circuit_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // --- Used Response UI ---

        public async Task<List<AB_Logic_Used_Response_Ui_Model>> GetUsedResponseUiAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Logic_Used_Response_Ui_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Used_Response_Ui_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddUsedResponseUiAsync(AB_Logic_Used_Response_Ui_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Logic_Used_Response_Ui_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Used_Response_Ui_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> DeleteUsedResponseUiAsync(AB_Logic_Used_Response_Ui_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Logic_Used_Response_Ui_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Logic_Used_Response_Ui_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // --- Sub Logics ---

        public async Task<List<AB_Logic_Sub_Logic_Model>> GetSubLogicsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Logic_Sub_Logics_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Sub_Logics_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddSubLogicAsync(AB_Logic_Sub_Logic_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Logic_Sub_Logic_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Sub_Logic_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> DeleteSubLogicAsync(AB_Logic_Sub_Logic_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Logic_Sub_Logic_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Logic_Sub_Logic_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // --- History Turns ---

        public async Task<List<AB_Logic_History_Turn_Model>> GetHistoryTurnsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Logic_History_Turns_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_History_Turns_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AppendHistoryTurnAsync(AB_Logic_History_Turn_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Append_Logic_History_Turn_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Append_Logic_History_Turn_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // ====================================================================
        // circuit-home-logic-graph-runtime-db-proxy sub 5 — v2 변수 슬롯 / 내부 노드 / 내부 connection
        // ====================================================================

        public async Task<List<AB_Logic_Variable_Slot_Model>> GetAllVariableSlotsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Logic_Variable_Slots_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Variable_Slots_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddVariableSlotAsync(AB_Logic_Variable_Slot_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Logic_Variable_Slot_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Variable_Slot_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> SaveVariableSlotAsync(AB_Logic_Variable_Slot_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Logic_Variable_Slot_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Logic_Variable_Slot_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> RemoveVariableSlotAsync(AB_Slot_Id _slotId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Remove_Logic_Variable_Slot_Request>();
            req.Slot_Id = _slotId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Remove_Logic_Variable_Slot_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<List<AB_Logic_Internal_Node_Model>> GetAllInternalNodesAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Logic_Internal_Nodes_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Internal_Nodes_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddInternalNodeAsync(AB_Logic_Internal_Node_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Logic_Internal_Node_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Internal_Node_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> RemoveInternalNodeAsync(long _nodeId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Remove_Logic_Internal_Node_Request>();
            req.Node_Id = _nodeId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Remove_Logic_Internal_Node_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> SaveInternalNodeAsync(AB_Logic_Internal_Node_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Logic_Internal_Node_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Logic_Internal_Node_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<List<AB_Logic_Internal_Connection_Model>> GetAllInternalConnectionsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Logic_Internal_Connections_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Internal_Connections_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddInternalConnectionAsync(AB_Logic_Internal_Connection_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Logic_Internal_Connection_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Internal_Connection_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> RemoveInternalConnectionAsync(long _connectionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Remove_Logic_Internal_Connection_Request>();
            req.Connection_Id = _connectionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Remove_Logic_Internal_Connection_Response>(req, DefaultTimeout);
            return resp.Success;
        }
    }
}
