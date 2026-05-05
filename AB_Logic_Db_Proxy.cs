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
    }
}
