using ArtificialBuilder.Models;
using ArtificialBuilder_EDP.Core.Messaging;
using System.Collections.Generic;

namespace ArtificialBuilder.Requests
{
    /// <summary>4 계층 저장소 ([[storage-layers]]) 단일 토픽. Resource / Session / Context / Node 통합.</summary>
    public static class AB_Storage_Topics
    {
        public const string Storage = "db.storage";
    }

    // ============================================================
    // Resource Storage — 페이로드 KV
    // ============================================================

    /// <summary>리소스 저장. id 발급 후 응답.</summary>
    public class AB_Add_Resource_Request : AB_Message
    {
        public string Kind = "text";
        public long Size;
        public string? PayloadInline;
        public string? PayloadPath;
        public AB_Add_Resource_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Add_Resource_Response : AB_Message
    {
        public long Id;
        public bool Success;
        public string? Error;
        public AB_Add_Resource_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>id 로 리소스 단건 조회.</summary>
    public class AB_Get_Resource_Request : AB_Message
    {
        public long Id;
        public AB_Get_Resource_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Get_Resource_Response : AB_Message
    {
        public AB_Resource_Storage_Model? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_Resource_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>리소스 삭제 (cascade 미적용 — 호출자가 참조 정리).</summary>
    public class AB_Delete_Resource_Request : AB_Message
    {
        public long Id;
        public AB_Delete_Resource_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Delete_Resource_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Resource_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    // ============================================================
    // Session Storage — turn 슬롯 1 직선 linked list
    // ============================================================

    /// <summary>새 turn 슬롯 추가 (list tail 에 append). prev/next 포인터 자동 연결.</summary>
    public class AB_Append_Session_Slot_Request : AB_Message
    {
        public string SessionId = "";
        public long? InputResourceId;
        public AB_Append_Session_Slot_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Append_Session_Slot_Response : AB_Message
    {
        public long Id;
        public bool Success;
        public string? Error;
        public AB_Append_Session_Slot_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>id 로 슬롯 조회.</summary>
    public class AB_Get_Session_Slot_Request : AB_Message
    {
        public long Id;
        public AB_Get_Session_Slot_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Get_Session_Slot_Response : AB_Message
    {
        public AB_Session_Storage_Model? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_Session_Slot_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>세션의 모든 슬롯 head→tail 순회 결과.</summary>
    public class AB_Get_Session_Slots_Request : AB_Message
    {
        public string SessionId = "";
        public AB_Get_Session_Slots_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Get_Session_Slots_Response : AB_Message
    {
        public List<AB_Session_Storage_Model> Data = new();
        public AB_Get_Session_Slots_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>슬롯의 active_context_id 갱신 (◀▶ 네비 / 새로고침 활성 변종 지정).</summary>
    public class AB_Set_Active_Context_Request : AB_Message
    {
        public long SlotId;
        public long? ActiveContextId;
        public AB_Set_Active_Context_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Set_Active_Context_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Set_Active_Context_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>슬롯 삭제 + prev/next 재연결.</summary>
    public class AB_Delete_Session_Slot_Request : AB_Message
    {
        public long Id;
        public AB_Delete_Session_Slot_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Delete_Session_Slot_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Session_Slot_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    // ============================================================
    // Context Storage — 1 회 실행 단위
    // ============================================================

    /// <summary>새 context (실행 변종) 추가. turn_id 기준 같은 슬롯 안의 변종.</summary>
    public class AB_Add_Context_Request : AB_Message
    {
        public string SessionId = "";
        public long TurnId;
        public AB_Add_Context_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Add_Context_Response : AB_Message
    {
        public long Id;
        public bool Success;
        public string? Error;
        public AB_Add_Context_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>turn 슬롯의 모든 변종 조회 (◀▶ 네비용).</summary>
    public class AB_Get_Contexts_By_Turn_Request : AB_Message
    {
        public long TurnId;
        public AB_Get_Contexts_By_Turn_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Get_Contexts_By_Turn_Response : AB_Message
    {
        public List<AB_Context_Storage_Model> Data = new();
        public AB_Get_Contexts_By_Turn_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>context 삭제 + 그 안의 Node 들 cascade.</summary>
    public class AB_Delete_Context_Request : AB_Message
    {
        public long Id;
        public AB_Delete_Context_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Delete_Context_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Context_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    // ============================================================
    // Node Storage — 1 노드 1 회 실행
    // ============================================================

    /// <summary>로직 실행 결과 추가. resource_id 단일 키만 보관.</summary>
    public class AB_Add_Node_Request : AB_Message
    {
        public long ContextId;
        public string NodeId = "";
        public int EmissionOrder;
        public long? ResourceId;
        public string? MetaJson;
        public AB_Add_Node_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Add_Node_Response : AB_Message
    {
        public long Id;
        public bool Success;
        public string? Error;
        public AB_Add_Node_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>context 의 모든 로직 실행 row 조회 (emission_order 정렬).</summary>
    public class AB_Get_Nodes_By_Context_Request : AB_Message
    {
        public long ContextId;
        public AB_Get_Nodes_By_Context_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Get_Nodes_By_Context_Response : AB_Message
    {
        public List<AB_Logic_Storage_Model> Data = new();
        public AB_Get_Nodes_By_Context_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>노드 단건 row 삭제. resource 는 orphan 그대로 (배경 정리는 별도 책임).</summary>
    public class AB_Delete_Node_Request : AB_Message
    {
        public long Id;
        public AB_Delete_Node_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Delete_Node_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Node_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    // ============================================================
    // 세션 cascade 삭제 — slots + contexts + nodes + 그것들이 가리키던 resources
    // ============================================================

    /// <summary>세션의 모든 신 storage row cascade 삭제. resource 는 해당 세션 데이터에서 참조하던 것만 같이 제거.</summary>
    public class AB_Delete_Session_Storage_Request : AB_Message
    {
        public string SessionId = "";
        public AB_Delete_Session_Storage_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Delete_Session_Storage_Response : AB_Message
    {
        public bool Success;
        public int SlotsRemoved;
        public int ContextsRemoved;
        public int NodesRemoved;
        public int ResourcesRemoved;
        public string? Error;
        public AB_Delete_Session_Storage_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    // ============================================================
    // Pending deletion 큐 — leaf-first time-budgeted sweep
    // ============================================================

    /// <summary>cascade 삭제 root 를 큐에 enqueue. session_storage / context_storage 만 enqueue (leaf 는 직접 delete).</summary>
    public class AB_Enqueue_Deletion_Request : AB_Message
    {
        public string TargetTable = "";
        public long TargetId;
        public AB_Enqueue_Deletion_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Enqueue_Deletion_Response : AB_Message
    {
        public bool Success;
        public AB_Enqueue_Deletion_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }

    /// <summary>1 회 sweep tick — budget 내에서 가능한 만큼 leaf delete. 매 틱 호출.</summary>
    public class AB_Sweep_Deletion_Step_Request : AB_Message
    {
        /// <summary>이번 tick 처리 시간 budget (microseconds).</summary>
        public long BudgetMicros;
        public AB_Sweep_Deletion_Step_Request() { Topic = AB_Storage_Topics.Storage; }
    }

    public class AB_Sweep_Deletion_Step_Response : AB_Message
    {
        /// <summary>이번 tick 에 삭제한 row 수 (resource + node + context + slot 합).</summary>
        public int RowsDeleted;
        /// <summary>큐가 비어있어 더 할 일 없음 (sweeper 가 sleep 모드 진입 가능).</summary>
        public bool QueueDrained;
        public AB_Sweep_Deletion_Step_Response() { Topic = AB_Storage_Topics.Storage; IsResponse = true; }
    }
}
