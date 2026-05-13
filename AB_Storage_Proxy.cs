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
    /// 4 계층 저장소 ([[storage-layers]]) 프록시. UI / Service / Node 가 사용하는 단일 진입점.
    /// 브로커 + AB_Storage_Gateway 경유. 직접 EDP_Db_Engine 호출 금지 ([[db-access]]).
    /// </summary>
    public class AB_Storage_Proxy : ArtificialBuilder_EDP.Core.AB_Object
    {
        private TimeSpan m_defaultTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan DefaultTimeout
        {
            get { return m_defaultTimeout; }
            set { m_defaultTimeout = value; }
        }

        private IAB_Message_Broker GetBroker()
            => ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();

        // ============================================================
        // Resource Storage
        // ============================================================

        /// <summary>리소스 저장. id 발급 후 반환.</summary>
        public async Task<long> AddResourceAsync(string _kind, long _size, string? _payloadInline, string? _payloadPath)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Resource_Request>();
            req.Kind = _kind;
            req.Size = _size;
            req.PayloadInline = _payloadInline;
            req.PayloadPath = _payloadPath;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Resource_Response>(req, DefaultTimeout);
            return resp.Success ? resp.Id : 0;
        }

        /// <summary>id 로 리소스 조회.</summary>
        public async Task<AB_Resource_Storage_Model?> GetResourceAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Resource_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Resource_Response>(req, DefaultTimeout);
            return resp.IsOk ? resp.Data : null;
        }

        /// <summary>리소스 삭제 (cascade 미적용 — 호출자가 참조 정리).</summary>
        public async Task<bool> DeleteResourceAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Resource_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Resource_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // ============================================================
        // Session Storage — turn 슬롯 1 직선 linked list
        // ============================================================

        /// <summary>세션 list 의 tail 에 새 turn 슬롯 추가. prev/next 포인터 자동 연결.</summary>
        public async Task<long> AppendSessionSlotAsync(long _sessionId, long? _inputResourceId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Append_Session_Slot_Request>();
            req.SessionId = _sessionId;
            req.InputResourceId = _inputResourceId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Append_Session_Slot_Response>(req, DefaultTimeout);
            return resp.Success ? resp.Id : 0;
        }

        /// <summary>id 로 슬롯 단건 조회.</summary>
        public async Task<AB_Session_Storage_Model?> GetSessionSlotAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Session_Slot_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Session_Slot_Response>(req, DefaultTimeout);
            return resp.IsOk ? resp.Data : null;
        }

        /// <summary>세션의 모든 슬롯 head→tail 순회.</summary>
        public async Task<List<AB_Session_Storage_Model>> GetSessionSlotsAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Session_Slots_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Session_Slots_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>슬롯의 active_context_id 갱신 (◀▶ 네비 / 새로고침 활성 변종 지정).</summary>
        public async Task<bool> SetActiveContextAsync(long _slotId, long? _activeContextId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Set_Active_Context_Request>();
            req.SlotId = _slotId;
            req.ActiveContextId = _activeContextId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Set_Active_Context_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>슬롯 삭제 + prev/next 재연결.</summary>
        public async Task<bool> DeleteSessionSlotAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Session_Slot_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Session_Slot_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // ============================================================
        // Context Storage — 1 회 실행 단위
        // ============================================================

        /// <summary>새 context (실행 변종) 추가. 같은 turn_id 의 변종으로 누적.</summary>
        public async Task<long> AddContextAsync(long _sessionId, long _turnId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Context_Request>();
            req.SessionId = _sessionId;
            req.TurnId = _turnId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Context_Response>(req, DefaultTimeout);
            return resp.Success ? resp.Id : 0;
        }

        /// <summary>turn 슬롯의 모든 변종 조회 (◀▶ 네비용, CreatedAt 정렬).</summary>
        public async Task<List<AB_Context_Storage_Model>> GetContextsByTurnAsync(long _turnId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Contexts_By_Turn_Request>();
            req.TurnId = _turnId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Contexts_By_Turn_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>context 삭제 + 그 안의 Node 들 cascade.</summary>
        public async Task<bool> DeleteContextAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Context_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Context_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // ============================================================
        // Node Storage — 1 노드 1 회 실행
        // ============================================================

        /// <summary>로직 실행 결과 추가. resource_id 단일 키만 보관. 2026-05-11 — NodeId string → long.</summary>
        public async Task<long> AddNodeAsync(long _contextId, long _nodeId, int _emissionOrder, long? _resourceId, string? _metaJson)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Node_Request>();
            req.ContextId = _contextId;
            req.NodeId = _nodeId;
            req.EmissionOrder = _emissionOrder;
            req.ResourceId = _resourceId;
            req.MetaJson = _metaJson;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Node_Response>(req, DefaultTimeout);
            return resp.Success ? resp.Id : 0;
        }

        /// <summary>context 의 모든 로직 실행 row 조회 (emission_order 정렬).</summary>
        public async Task<List<AB_Logic_Storage_Model>> GetNodesByContextAsync(long _contextId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Nodes_By_Context_Request>();
            req.ContextId = _contextId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Nodes_By_Context_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>노드 단건 row 삭제. resource 는 orphan 그대로 (단순 분리 정책 — read 경로는 즉시 안 보임).</summary>
        public async Task<bool> DeleteNodeAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Node_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Node_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // ============================================================
        // Pending deletion 큐 — leaf-first time-budgeted sweep
        // ============================================================

        /// <summary>cascade 삭제 root 를 큐에 enqueue (즉시 분리, sweeper 가 leaf-first 정리).</summary>
        public async Task<bool> EnqueueDeletionAsync(string _targetTable, long _targetId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Enqueue_Deletion_Request>();
            req.TargetTable = _targetTable;
            req.TargetId = _targetId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Enqueue_Deletion_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>1 회 sweep tick — 주어진 budget 안에서 leaf 하나씩 삭제. Sweeper 컴포넌트가 매 틱 호출.</summary>
        public async Task<(int RowsDeleted, bool Drained)> SweepDeletionStepAsync(long _budgetMicros)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Sweep_Deletion_Step_Request>();
            req.BudgetMicros = _budgetMicros;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Sweep_Deletion_Step_Response>(req, DefaultTimeout);
            return (resp.RowsDeleted, resp.QueueDrained);
        }

        // ============================================================
        // 세션 cascade 삭제
        // ============================================================

        /// <summary>세션의 모든 신 storage row cascade 삭제 (slots + contexts + nodes + 그것들이 가리키던 resources).</summary>
        public async Task<AB_Delete_Session_Storage_Response> DeleteSessionStorageAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Session_Storage_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Session_Storage_Response>(req, DefaultTimeout);
            return resp;
        }
    }
}
