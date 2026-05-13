using ArtificialBuilder.Models;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Collections.Generic;

// Phase E — wire-safety 감사 (2026-04-16)
// - POCO 필드는 JSON 직렬화 가능 타입 (string / 기본 / 내장 Model) 만 사용.
// - object?/인터페이스/delegate/Expression 필드 없음. OK.
// - 대용량 payload 주의:
//   * AB_Persona_Settings_Model.IconData_ (byte[]?) — 작은 아이콘, 인라인 유지 가능.
//   * AB_Chat_Message_Model.ImageData_ (string base64) — 수 MB 가능.
//     TODO(redis): Redis 교체 시 base64 인라인 대신 파일시스템 블롭 경로 참조로 전환.
//   * AB_Saved_Image_Model.Embedding_ (byte[]?) — 수 KB 벡터, 인라인 OK.
// - AB_In_Memory_Broker 는 객체참조 전달이라 in-proc 에선 비용 없음 (Phase E 범위 내 유지).
namespace ArtificialBuilder.Requests
{
    /// <summary>Persona DB 브로커 토픽.</summary>
    public static class AB_Persona_Db_Topics
    {
        public const string Persona = "db.persona";
    }

    // --- GetSettings ---

    /// <summary>페르소나 설정 조회.</summary>
    public class AB_Get_Persona_Settings_Request : AB_Message
    {
        public AB_Get_Persona_Settings_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }

    /// <summary>페르소나 설정 응답.</summary>
    public class AB_Get_Persona_Settings_Response : AB_Message
    {
        public AB_Persona_Settings_Model? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_Persona_Settings_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    // --- AddSettings ---

    /// <summary>페르소나 설정 신규 추가.</summary>
    public class AB_Add_Persona_Settings_Request : AB_Message
    {
        public AB_Persona_Settings_Model Settings = new();
        public AB_Add_Persona_Settings_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }

    /// <summary>설정 추가 응답.</summary>
    public class AB_Add_Persona_Settings_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Add_Persona_Settings_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    // --- SaveSettings ---

    /// <summary>페르소나 설정 저장.</summary>
    public class AB_Save_Persona_Settings_Request : AB_Message
    {
        public AB_Persona_Settings_Model Settings = new();
        public AB_Save_Persona_Settings_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }

    /// <summary>설정 저장 응답.</summary>
    public class AB_Save_Persona_Settings_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Save_Persona_Settings_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    // --- GetActiveName ---

    /// <summary>현재 활성 페르소나 이름 조회.</summary>
    public class AB_Get_Active_Persona_Name_Request : AB_Message
    {
        public AB_Get_Active_Persona_Name_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }

    /// <summary>활성 페르소나 이름 응답.</summary>
    public class AB_Get_Active_Persona_Name_Response : AB_Message
    {
        public new string? Name;
        public AB_Get_Active_Persona_Name_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    // --- GetPersonaNames ---

    /// <summary>모든 페르소나 이름 조회.</summary>
    public class AB_Get_Persona_Names_Request : AB_Message
    {
        public AB_Get_Persona_Names_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }

    /// <summary>페르소나 이름 목록 응답.</summary>
    public class AB_Get_Persona_Names_Response : AB_Message
    {
        public List<string> Names = new();
        public AB_Get_Persona_Names_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    // --- RenamePersona (Close → File.Move → Open) ---

    /// <summary>페르소나 이름 변경 (Close + File.Move + Open).</summary>
    public class AB_Rename_Persona_Request : AB_Message
    {
        public string OldName = "";
        public string NewName = "";
        public AB_Rename_Persona_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }

    /// <summary>이름 변경 응답.</summary>
    public class AB_Rename_Persona_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Rename_Persona_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    // --- DeletePersona (Close → File.Delete → Open next) ---

    /// <summary>페르소나 삭제 (Close + File.Delete + Open next).</summary>
    public class AB_Delete_Persona_Request : AB_Message
    {
        public string PersonaName = "";
        public AB_Delete_Persona_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }

    /// <summary>삭제 응답. NextPersonaName = null 이면 마지막 페르소나 삭제 실패.</summary>
    public class AB_Delete_Persona_Response : AB_Message
    {
        public bool Success;
        public string? NextPersonaName;
        public string? Error;
        public AB_Delete_Persona_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    // ================================================================
    // Session CRUD
    // ================================================================

    public class AB_Create_Session_Request : AB_Message
    {
        public string PersonaName = "";
        public string CircuitName = "";
        /// <summary>세션 TurnShardSize 초기값. 0 이하면 모델 기본값(50) 사용.</summary>
        public int TurnShardSize = 0;
        public AB_Create_Session_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Create_Session_Response : AB_Message
    {
        public AB_Chat_Session_Model? Data;
        public AB_Create_Session_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Get_Session_Request : AB_Message
    {
        public long SessionId;
        public AB_Get_Session_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Get_Session_Response : AB_Message
    {
        public AB_Chat_Session_Model? Data;
        public AB_Get_Session_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Get_All_Sessions_Request : AB_Message
    {
        public string Filter = "";
        public AB_Get_All_Sessions_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Get_All_Sessions_Response : AB_Message
    {
        public List<AB_Chat_Session_Model> Data = new();
        public AB_Get_All_Sessions_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Get_Sessions_Request : AB_Message
    {
        public string CircuitName = "";
        public string Filter = "";
        public AB_Get_Sessions_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Get_Sessions_Response : AB_Message
    {
        public List<AB_Chat_Session_Model> Data = new();
        public AB_Get_Sessions_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Get_Session_Count_Request : AB_Message
    {
        public string CircuitName = "";
        public AB_Get_Session_Count_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Get_Session_Count_Response : AB_Message
    {
        public int Count;
        public AB_Get_Session_Count_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Rename_Session_Request : AB_Message
    {
        public long SessionId;
        public string NewTitle = "";
        public AB_Rename_Session_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Rename_Session_Response : AB_Message
    {
        public bool Success;
        public AB_Rename_Session_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Copy_Session_Request : AB_Message
    {
        public long SessionId;
        public AB_Copy_Session_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Copy_Session_Response : AB_Message
    {
        public AB_Chat_Session_Model? Data;
        public AB_Copy_Session_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Move_Session_Request : AB_Message
    {
        public long SessionId;
        public string TargetPersonaName = "";
        public AB_Move_Session_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Move_Session_Response : AB_Message
    {
        public bool Success;
        public AB_Move_Session_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Delete_Session_Request : AB_Message
    {
        public long SessionId;
        public AB_Delete_Session_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Delete_Session_Response : AB_Message
    {
        public bool Success;
        public AB_Delete_Session_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Touch_Session_Request : AB_Message
    {
        public long SessionId;
        public AB_Touch_Session_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Touch_Session_Response : AB_Message
    {
        public bool Success;
        public AB_Touch_Session_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Update_Session_Title_From_First_Message_Request : AB_Message
    {
        public long SessionId;
        public string Text = "";
        public AB_Update_Session_Title_From_First_Message_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Update_Session_Title_From_First_Message_Response : AB_Message
    {
        public string? Title;
        public AB_Update_Session_Title_From_First_Message_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Update_Session_Cost_Request : AB_Message
    {
        public long SessionId;
        public long InputTokens;
        public long OutputTokens;
        public decimal Cost;
        public AB_Update_Session_Cost_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Update_Session_Cost_Response : AB_Message
    {
        public bool Success;
        public AB_Update_Session_Cost_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Get_Session_Cost_Request : AB_Message
    {
        public long SessionId;
        public AB_Get_Session_Cost_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Get_Session_Cost_Response : AB_Message
    {
        public long InputTokens;
        public long OutputTokens;
        public decimal Cost;
        public AB_Get_Session_Cost_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    // ================================================================
    // Phase 4.4.d — SessionDataPool 요청/응답 폐기. 4 계층 storage 가 정본.

    // Phase 4.6 — AB_Delete_Context_By_Session_Request/Response 폐기. 4 계층 storage cascade 가 정본.

    /// <summary>
    /// 세션의 TurnShardSize 값을 변경. 샤드 경계가 바뀌므로 기존 context_records/history 는
    /// 호출자가 wipe 후 진행할 것 (이 요청은 chat_sessions 행만 갱신).
    /// </summary>
    public class AB_Update_Session_Turn_Shard_Size_Request : AB_Message
    {
        public long SessionId;
        public int NewTurnShardSize = 50;
        public AB_Update_Session_Turn_Shard_Size_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Update_Session_Turn_Shard_Size_Response : AB_Message
    {
        public bool Success;
        public AB_Update_Session_Turn_Shard_Size_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    /// <summary>
    /// 세션의 CircuitName_ 만 갱신. chat 진행 중 circuit 교체 시 사용 — 다음 turn 부터 새 circuit 적용.
    /// history 데이터 보존, 페이로드/스토리지 wipe 없음.
    /// </summary>
    public class AB_Update_Session_Circuit_Request : AB_Message
    {
        public long SessionId;
        public string CircuitName = "";
        public AB_Update_Session_Circuit_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Update_Session_Circuit_Response : AB_Message
    {
        public bool Success;
        public AB_Update_Session_Circuit_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    // Phase 4.6 — Context_Record / Context_History Request/Response 통째 폐기.
    // 4 계층 storage 의 Context_Storage / Node_Storage / Session_Storage 가 정본.
    // MessageDataRef Request/Response 는 Phase C 에서 폐기됨.

    // ================================================================
    // SavedImage
    // ================================================================

    public class AB_Add_Saved_Image_Request : AB_Message
    {
        public AB_Saved_Image_Model? Image;
        public AB_Add_Saved_Image_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Add_Saved_Image_Response : AB_Message
    {
        public bool Success;
        public AB_Add_Saved_Image_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    // ================================================================
    // Vec (Chat Embedding) — Persona owns chat embeddings
    // ================================================================

    public class AB_Persona_Search_Chat_Request : AB_Message
    {
        public float[] Query = System.Array.Empty<float>();
        public int TopK;
        public string? ExcludeSessionId;
        public AB_Persona_Search_Chat_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Persona_Search_Chat_Response : AB_Message
    {
        public List<AB_Vec_Chat_Hit> Hits = new();
        public AB_Persona_Search_Chat_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Persona_Insert_Chat_Embedding_Request : AB_Message
    {
        public long SessionId;
        public long NodeId;
        public int TurnIndex;
        public int RefreshIndex;
        public int EmissionOrder;
        public float[] Embedding = System.Array.Empty<float>();
        public AB_Persona_Insert_Chat_Embedding_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Persona_Insert_Chat_Embedding_Response : AB_Message
    {
        public bool Success;
        public AB_Persona_Insert_Chat_Embedding_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Persona_Delete_Chat_Embeddings_By_Session_Request : AB_Message
    {
        public long SessionId;
        public AB_Persona_Delete_Chat_Embeddings_By_Session_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Persona_Delete_Chat_Embeddings_By_Session_Response : AB_Message
    {
        public bool Success;
        public AB_Persona_Delete_Chat_Embeddings_By_Session_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Persona_Delete_Chat_Embedding_By_Record_Request : AB_Message
    {
        public long SessionId;
        public long NodeId;
        public int TurnIndex;
        public int RefreshIndex;
        public int EmissionOrder;
        public AB_Persona_Delete_Chat_Embedding_By_Record_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Persona_Delete_Chat_Embedding_By_Record_Response : AB_Message
    {
        public bool Success;
        public AB_Persona_Delete_Chat_Embedding_By_Record_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }

    public class AB_Persona_Get_Chat_Embeddings_By_Session_Request : AB_Message
    {
        public long SessionId;
        public AB_Persona_Get_Chat_Embeddings_By_Session_Request() { Topic = AB_Persona_Db_Topics.Persona; }
    }
    public class AB_Persona_Get_Chat_Embeddings_By_Session_Response : AB_Message
    {
        public List<AB_Chat_Embedding_Info> Data = new();
        public AB_Persona_Get_Chat_Embeddings_By_Session_Response() { Topic = AB_Persona_Db_Topics.Persona; IsResponse = true; }
    }
}
