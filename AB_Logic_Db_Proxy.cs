using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// Logic DB 외부 진입 프록시 (얇은 wrapper). 메시지 브로커 + AB_Logic_Db_Gateway 경유 호출.
    /// 사용:
    ///   var proxy = AB_Logic_Db_Proxy.I;
    ///   var meta = await proxy.GetMetaAsync();
    /// </summary>
    public class AB_Logic_Db_Proxy
    {
        private static AB_Logic_Db_Proxy? g_instance;
        /// <summary>전역 단일 인스턴스.</summary>
        public static AB_Logic_Db_Proxy I => g_instance ??= new AB_Logic_Db_Proxy();

        private TimeSpan m_defaultTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan DefaultTimeout
        {
            get { return m_defaultTimeout; }
            set { m_defaultTimeout = value; }
        }

        private IAB_Message_Broker GetBroker()
            => ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();

        // --- 로직 라이브러리 (main-tabs-and-package-system sub 2) ---

        public async Task<string?> CreateLogicAsync(string? _name = null, string? _uuid = null)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Create_Logic_Response>(
                new AB_Create_Logic_Request { Uuid = _uuid, Name = _name }, DefaultTimeout);
            return resp.Uuid;
        }

        public async Task<bool> DeleteLogicAsync(string _uuid)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Logic_Response>(
                new AB_Delete_Logic_Request { Uuid = _uuid }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<List<AB_Logic_Library_Item>> GetLogicLibraryInfoAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Logic_Library_Info_Response>(
                new AB_Get_Logic_Library_Info_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> OpenLogicAsync(string _uuid)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Open_Logic_Response>(
                new AB_Open_Logic_Request { Uuid = _uuid }, DefaultTimeout);
            return resp.Success;
        }

        // --- Meta ---

        public async Task<AB_Logic_Meta_Model?> GetMetaAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Logic_Meta_Response>(
                new AB_Get_Logic_Meta_Request(), DefaultTimeout);
            return resp.Data;
        }

        public async Task<bool> SaveMetaAsync(AB_Logic_Meta_Model _meta)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Logic_Meta_Response>(
                new AB_Save_Logic_Meta_Request { Meta = _meta }, DefaultTimeout);
            return resp.Success;
        }

        // --- Used Circuits ---

        public async Task<List<AB_Logic_Used_Circuit_Model>> GetUsedCircuitsAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Used_Circuits_Response>(
                new AB_Get_All_Logic_Used_Circuits_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddUsedCircuitAsync(AB_Logic_Used_Circuit_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Used_Circuit_Response>(
                new AB_Add_Logic_Used_Circuit_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> SaveUsedCircuitAsync(AB_Logic_Used_Circuit_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Logic_Used_Circuit_Response>(
                new AB_Save_Logic_Used_Circuit_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> DeleteUsedCircuitAsync(AB_Logic_Used_Circuit_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Logic_Used_Circuit_Response>(
                new AB_Delete_Logic_Used_Circuit_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        // --- Used Response UI ---

        public async Task<List<AB_Logic_Used_Response_Ui_Model>> GetUsedResponseUiAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Used_Response_Ui_Response>(
                new AB_Get_All_Logic_Used_Response_Ui_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddUsedResponseUiAsync(AB_Logic_Used_Response_Ui_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Used_Response_Ui_Response>(
                new AB_Add_Logic_Used_Response_Ui_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> DeleteUsedResponseUiAsync(AB_Logic_Used_Response_Ui_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Logic_Used_Response_Ui_Response>(
                new AB_Delete_Logic_Used_Response_Ui_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        // --- Sub Logics ---

        public async Task<List<AB_Logic_Sub_Logic_Model>> GetSubLogicsAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Sub_Logics_Response>(
                new AB_Get_All_Logic_Sub_Logics_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddSubLogicAsync(AB_Logic_Sub_Logic_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Sub_Logic_Response>(
                new AB_Add_Logic_Sub_Logic_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> DeleteSubLogicAsync(AB_Logic_Sub_Logic_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Logic_Sub_Logic_Response>(
                new AB_Delete_Logic_Sub_Logic_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        // --- History Turns ---

        public async Task<List<AB_Logic_History_Turn_Model>> GetHistoryTurnsAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_History_Turns_Response>(
                new AB_Get_All_Logic_History_Turns_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AppendHistoryTurnAsync(AB_Logic_History_Turn_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Append_Logic_History_Turn_Response>(
                new AB_Append_Logic_History_Turn_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        // ====================================================================
        // circuit-home-logic-graph-runtime-db-proxy sub 5 — v2 변수 슬롯 / 내부 노드 / 내부 connection
        // ====================================================================

        public async Task<List<AB_Logic_Variable_Slot_Model>> GetAllVariableSlotsAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Variable_Slots_Response>(
                new AB_Get_All_Logic_Variable_Slots_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddVariableSlotAsync(AB_Logic_Variable_Slot_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Variable_Slot_Response>(
                new AB_Add_Logic_Variable_Slot_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> SaveVariableSlotAsync(AB_Logic_Variable_Slot_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Logic_Variable_Slot_Response>(
                new AB_Save_Logic_Variable_Slot_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> RemoveVariableSlotAsync(string _slotId)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Remove_Logic_Variable_Slot_Response>(
                new AB_Remove_Logic_Variable_Slot_Request { Slot_Id = _slotId }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<List<AB_Logic_Internal_Node_Model>> GetAllInternalNodesAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Internal_Nodes_Response>(
                new AB_Get_All_Logic_Internal_Nodes_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddInternalNodeAsync(AB_Logic_Internal_Node_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Internal_Node_Response>(
                new AB_Add_Logic_Internal_Node_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> RemoveInternalNodeAsync(string _nodeId)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Remove_Logic_Internal_Node_Response>(
                new AB_Remove_Logic_Internal_Node_Request { Node_Id = _nodeId }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<List<AB_Logic_Internal_Connection_Model>> GetAllInternalConnectionsAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Logic_Internal_Connections_Response>(
                new AB_Get_All_Logic_Internal_Connections_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddInternalConnectionAsync(AB_Logic_Internal_Connection_Model _item)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Logic_Internal_Connection_Response>(
                new AB_Add_Logic_Internal_Connection_Request { Item = _item }, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> RemoveInternalConnectionAsync(string _connectionId)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Remove_Logic_Internal_Connection_Response>(
                new AB_Remove_Logic_Internal_Connection_Request { Connection_Id = _connectionId }, DefaultTimeout);
            return resp.Success;
        }
    }
}
