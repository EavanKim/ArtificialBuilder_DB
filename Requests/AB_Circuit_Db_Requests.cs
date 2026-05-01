using ArtificialBuilder.Models;
using ArtificialBuilder_EDP.Core.Messaging;
using System.Collections.Generic;

// Phase E — wire-safety 감사 (2026-04-16)
// - POCO 필드는 JSON 직렬화 가능 타입 (string / 기본 / 내장 Model) 만 사용.
// - object?/인터페이스/delegate/Expression 필드 없음. OK.
// - 대용량 payload 주의:
//   * AB_Circuit_Asset_Model.Data_ (byte[]) — 에셋 바이너리. 최대 수 MB. 현재 in-proc 객체참조로 전달.
//     TODO(redis): Redis 교체 시 에셋 ID 참조만 wire 로 보내고, 바이트는 파일시스템 블롭으로 이주.
//   * AB_Get_Asset_Data_Response.Data (byte[]?) — 위와 동일 payload.
//     TODO(redis): 경로 참조(string) 로 대체하거나 별도 blob fetch 경로 도입.
//   * AB_Circuit_Settings_Model.IconData_ (byte[]?) — 작은 아이콘, 인라인 유지 가능.
// - AB_In_Memory_Broker 는 객체참조 전달이라 in-proc 에선 복사 비용 없음 (Phase E 범위 내 유지).
namespace ArtificialBuilder.Requests
{
    /// <summary>
    /// Circuit DB 게이트웨이가 처리하는 메시지 토픽 상수.
    /// 토픽 분리는 핸들 단위가 아닌 도메인 단위 — 단일 active circuit 시점에서는 토픽 1개로 충분.
    /// 향후 다중 컨텍스트 시 "db.circuit.{handle}" 형태로 확장.
    /// </summary>
    public static class AB_Circuit_Db_Topics
    {
        /// <summary>현재 활성 circuit DB에 대한 모든 요청.</summary>
        public const string ActiveCircuit = "db.circuit.active";
    }

    // --- GetAllCharacters ---

    /// <summary>활성 circuit의 캐릭터 목록 조회 요청.</summary>
    public class AB_Get_All_Characters_Request : AB_Message
    {
        /// <summary>세션 ID 필터 (null이면 전체).</summary>
        public string? SessionId;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_All_Characters_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>캐릭터 목록 조회 응답.</summary>
    public class AB_Get_All_Characters_Response : AB_Message
    {
        /// <summary>결과 캐릭터 리스트.</summary>
        public List<AB_Character_Model> Data = new();
        /// <summary>오류 메시지 (성공 시 null).</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_All_Characters_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetCharacter (단일) ---

    /// <summary>ID로 단일 캐릭터 조회.</summary>
    public class AB_Get_Character_Request : AB_Message
    {
        /// <summary>캐릭터 ID.</summary>
        public string Id = "";
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_Character_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>단일 캐릭터 응답.</summary>
    public class AB_Get_Character_Response : AB_Message
    {
        /// <summary>결과 (없으면 null).</summary>
        public AB_Character_Model? Data;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_Character_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- AddCharacter ---

    /// <summary>캐릭터 추가 요청.</summary>
    public class AB_Add_Character_Request : AB_Message
    {
        /// <summary>추가할 캐릭터.</summary>
        public AB_Character_Model Character = new();
        /// <summary>세션 ID (옵션).</summary>
        public string? SessionId;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Add_Character_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>AddCharacter 응답.</summary>
    public class AB_Add_Character_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Add_Character_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- SaveCharacter ---

    /// <summary>캐릭터 갱신 요청.</summary>
    public class AB_Save_Character_Request : AB_Message
    {
        /// <summary>저장할 캐릭터.</summary>
        public AB_Character_Model Character = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Save_Character_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>SaveCharacter 응답.</summary>
    public class AB_Save_Character_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Save_Character_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- DeleteCharacter ---

    /// <summary>캐릭터 삭제 요청.</summary>
    public class AB_Delete_Character_Request : AB_Message
    {
        /// <summary>삭제할 캐릭터.</summary>
        public AB_Character_Model Character = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Delete_Character_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>DeleteCharacter 응답.</summary>
    public class AB_Delete_Character_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Delete_Character_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAllRelationships ---

    /// <summary>관계 전체 조회 요청.</summary>
    public class AB_Get_All_Relationships_Request : AB_Message
    {
        /// <summary>세션 ID 필터.</summary>
        public string? SessionId;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Relationships_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>관계 응답.</summary>
    public class AB_Get_All_Relationships_Response : AB_Message
    {
        /// <summary>관계 리스트.</summary>
        public List<AB_Character_Relationship_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Relationships_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAllLocations ---

    /// <summary>장소 전체 조회 요청.</summary>
    public class AB_Get_All_Locations_Request : AB_Message
    {
        /// <summary>세션 ID 필터.</summary>
        public string? SessionId;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Locations_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>장소 응답.</summary>
    public class AB_Get_All_Locations_Response : AB_Message
    {
        /// <summary>장소 리스트.</summary>
        public List<AB_Location_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Locations_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAllLocationConnections ---

    /// <summary>장소 연결 전체 조회.</summary>
    public class AB_Get_All_Location_Connections_Request : AB_Message
    {
        /// <summary>세션 ID 필터.</summary>
        public string? SessionId;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Location_Connections_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>장소 연결 응답.</summary>
    public class AB_Get_All_Location_Connections_Response : AB_Message
    {
        /// <summary>장소 연결 리스트.</summary>
        public List<AB_Location_Connection_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Location_Connections_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAllRelationColors / Add / Save / Delete ---

    /// <summary>관계 색상 전체 조회.</summary>
    public class AB_Get_All_Relation_Colors_Request : AB_Message
    {
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Relation_Colors_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>관계 색상 응답.</summary>
    public class AB_Get_All_Relation_Colors_Response : AB_Message
    {
        /// <summary>색상 리스트.</summary>
        public List<AB_Relation_Color_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Relation_Colors_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>관계 색상 추가.</summary>
    public class AB_Add_Relation_Color_Request : AB_Message
    {
        /// <summary>추가할 색상.</summary>
        public AB_Relation_Color_Model Color = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Add_Relation_Color_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>AddRelationColor 응답.</summary>
    public class AB_Add_Relation_Color_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Add_Relation_Color_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>관계 색상 갱신.</summary>
    public class AB_Save_Relation_Color_Request : AB_Message
    {
        /// <summary>저장할 색상.</summary>
        public AB_Relation_Color_Model Color = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Save_Relation_Color_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>SaveRelationColor 응답.</summary>
    public class AB_Save_Relation_Color_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Save_Relation_Color_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>관계 색상 삭제.</summary>
    public class AB_Delete_Relation_Color_Request : AB_Message
    {
        /// <summary>삭제할 색상.</summary>
        public AB_Relation_Color_Model Color = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Delete_Relation_Color_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>DeleteRelationColor 응답.</summary>
    public class AB_Delete_Relation_Color_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Delete_Relation_Color_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAllPatterns ---

    /// <summary>활성 circuit의 패턴 목록 조회 요청.</summary>
    public class AB_Get_All_Patterns_Request : AB_Message
    {
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Patterns_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>패턴 목록 조회 응답.</summary>
    public class AB_Get_All_Patterns_Response : AB_Message
    {
        /// <summary>결과 패턴 리스트.</summary>
        public List<AB_Pattern_Config_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Patterns_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- SessionData / CharacterData / DataCategory ---

    /// <summary>세션 캐릭터 데이터 전체 조회 요청.</summary>
    public class AB_Get_All_Session_Data_Request : AB_Message
    {
        /// <summary>세션 ID.</summary>
        public string SessionId = "";
        /// <summary>토픽.</summary>
        public AB_Get_All_Session_Data_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>세션 캐릭터 데이터 전체 조회 응답.</summary>
    public class AB_Get_All_Session_Data_Response : AB_Message
    {
        /// <summary>결과.</summary>
        public List<AB_Character_Data_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Get_All_Session_Data_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>데이터 카테고리 전체 조회 요청.</summary>
    public class AB_Get_All_Data_Categories_Request : AB_Message
    {
        /// <summary>토픽.</summary>
        public AB_Get_All_Data_Categories_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>데이터 카테고리 전체 조회 응답.</summary>
    public class AB_Get_All_Data_Categories_Response : AB_Message
    {
        /// <summary>결과.</summary>
        public List<AB_Data_Category_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Get_All_Data_Categories_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>세션 데이터 일괄 삭제 요청.</summary>
    public class AB_Delete_Session_Data_Request : AB_Message
    {
        /// <summary>세션 ID.</summary>
        public string SessionId = "";
        /// <summary>토픽.</summary>
        public AB_Delete_Session_Data_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>세션 데이터 삭제 응답.</summary>
    public class AB_Delete_Session_Data_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Delete_Session_Data_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>특정 메시지의 캐릭터 데이터 삭제 요청.</summary>
    public class AB_Delete_Character_Data_By_Message_Request : AB_Message
    {
        /// <summary>메시지 ID.</summary>
        public long MessageId;
        /// <summary>토픽.</summary>
        public AB_Delete_Character_Data_By_Message_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Delete_Character_Data_By_Message_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Delete_Character_Data_By_Message_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>지정 메시지 이후 캐릭터 데이터 삭제 요청.</summary>
    public class AB_Delete_Character_Data_From_Message_Request : AB_Message
    {
        /// <summary>시작 메시지 ID.</summary>
        public long FromMessageId;
        /// <summary>토픽.</summary>
        public AB_Delete_Character_Data_From_Message_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Delete_Character_Data_From_Message_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Delete_Character_Data_From_Message_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>Circuit 데이터를 세션으로 복사 요청.</summary>
    public class AB_Copy_Circuit_Data_To_Session_Request : AB_Message
    {
        /// <summary>대상 세션 ID.</summary>
        public string SessionId = "";
        /// <summary>토픽.</summary>
        public AB_Copy_Circuit_Data_To_Session_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Copy_Circuit_Data_To_Session_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Copy_Circuit_Data_To_Session_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>카테고리별 캐릭터 데이터 조회 요청.</summary>
    public class AB_Get_Character_Data_By_Category_Request : AB_Message
    {
        /// <summary>캐릭터 ID.</summary>
        public string CharacterId = "";
        /// <summary>세션 ID (null=Circuit 템플릿).</summary>
        public string? SessionId;
        /// <summary>카테고리 ID.</summary>
        public string CategoryId = "";
        /// <summary>토픽.</summary>
        public AB_Get_Character_Data_By_Category_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Get_Character_Data_By_Category_Response : AB_Message
    {
        /// <summary>결과.</summary>
        public List<AB_Character_Data_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Get_Character_Data_By_Category_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>캐릭터 데이터 upsert 요청.</summary>
    public class AB_Upsert_Character_Data_Request : AB_Message
    {
        /// <summary>캐릭터 ID.</summary>
        public string CharacterId = "";
        /// <summary>세션 ID.</summary>
        public string? SessionId;
        /// <summary>카테고리 ID.</summary>
        public string CategoryId = "";
        /// <summary>필드 이름.</summary>
        public string FieldName = "";
        /// <summary>필드 값.</summary>
        public string? FieldValue;
        /// <summary>설명.</summary>
        public string? Narrative;
        /// <summary>출처.</summary>
        public string Source = "";
        /// <summary>메시지 ID.</summary>
        public long? MessageId;
        /// <summary>토픽.</summary>
        public AB_Upsert_Character_Data_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Upsert_Character_Data_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Upsert_Character_Data_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>캐릭터 데이터 단건 삭제 요청.</summary>
    public class AB_Delete_Character_Data_Request : AB_Message
    {
        /// <summary>대상 ID.</summary>
        public string Id = "";
        /// <summary>토픽.</summary>
        public AB_Delete_Character_Data_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Delete_Character_Data_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Delete_Character_Data_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>데이터 카테고리 추가 요청.</summary>
    public class AB_Add_Data_Category_Request : AB_Message
    {
        /// <summary>대상.</summary>
        public AB_Data_Category_Model Category = new();
        /// <summary>토픽.</summary>
        public AB_Add_Data_Category_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Add_Data_Category_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Add_Data_Category_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- Response Window CRUD ---

    /// <summary>응답 윈도우 전체 조회 요청.</summary>
    public class AB_Get_All_Windows_Request : AB_Message
    {
        /// <summary>토픽.</summary>
        public AB_Get_All_Windows_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Get_All_Windows_Response : AB_Message
    {
        /// <summary>결과.</summary>
        public List<AB_Response_Window_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Get_All_Windows_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>응답 윈도우 단건 조회 요청.</summary>
    public class AB_Get_Window_Request : AB_Message
    {
        /// <summary>윈도우 ID.</summary>
        public string Id = "";
        /// <summary>토픽.</summary>
        public AB_Get_Window_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Get_Window_Response : AB_Message
    {
        /// <summary>결과 (없으면 null).</summary>
        public AB_Response_Window_Model? Data;
        /// <summary>존재 여부.</summary>
        public bool IsOk;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Get_Window_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>응답 윈도우 추가 요청.</summary>
    public class AB_Add_Window_Request : AB_Message
    {
        /// <summary>대상.</summary>
        public AB_Response_Window_Model Window = new();
        /// <summary>토픽.</summary>
        public AB_Add_Window_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Add_Window_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Add_Window_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>응답 윈도우 갱신 요청.</summary>
    public class AB_Save_Window_Request : AB_Message
    {
        /// <summary>대상.</summary>
        public AB_Response_Window_Model Window = new();
        /// <summary>토픽.</summary>
        public AB_Save_Window_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Save_Window_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Save_Window_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>응답 윈도우 삭제 요청.</summary>
    public class AB_Delete_Window_Request : AB_Message
    {
        /// <summary>윈도우 ID.</summary>
        public string Id = "";
        /// <summary>토픽.</summary>
        public AB_Delete_Window_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Delete_Window_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Delete_Window_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- WindowComponents — AB_Window 조립 컴포넌트 CRUD ---

    /// <summary>특정 윈도우의 컴포넌트 전체 조회 요청.</summary>
    public class AB_Get_Window_Components_Request : AB_Message
    {
        /// <summary>부모 윈도우 ID.</summary>
        public string WindowId = "";
        /// <summary>토픽.</summary>
        public AB_Get_Window_Components_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Get_Window_Components_Response : AB_Message
    {
        /// <summary>결과.</summary>
        public List<AB_Window_Component_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Get_Window_Components_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>전 윈도우의 컴포넌트 통합 조회 요청.</summary>
    public class AB_Get_All_Window_Components_Request : AB_Message
    {
        /// <summary>토픽.</summary>
        public AB_Get_All_Window_Components_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Get_All_Window_Components_Response : AB_Message
    {
        /// <summary>결과.</summary>
        public List<AB_Window_Component_Model> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Get_All_Window_Components_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>윈도우 컴포넌트 추가 요청.</summary>
    public class AB_Add_Window_Component_Request : AB_Message
    {
        /// <summary>대상.</summary>
        public AB_Window_Component_Model Component = new();
        /// <summary>토픽.</summary>
        public AB_Add_Window_Component_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Add_Window_Component_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Add_Window_Component_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>윈도우 컴포넌트 갱신 요청.</summary>
    public class AB_Save_Window_Component_Request : AB_Message
    {
        /// <summary>대상.</summary>
        public AB_Window_Component_Model Component = new();
        /// <summary>토픽.</summary>
        public AB_Save_Window_Component_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Save_Window_Component_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Save_Window_Component_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>윈도우 컴포넌트 삭제 요청.</summary>
    public class AB_Delete_Window_Component_Request : AB_Message
    {
        /// <summary>삭제 대상 컴포넌트 ID.</summary>
        public string Id = "";
        /// <summary>토픽.</summary>
        public AB_Delete_Window_Component_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Delete_Window_Component_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Delete_Window_Component_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>윈도우의 전 컴포넌트 cascade 삭제 (DeleteWindow 동반).</summary>
    public class AB_Delete_Window_Components_By_Window_Request : AB_Message
    {
        /// <summary>부모 윈도우 ID.</summary>
        public string WindowId = "";
        /// <summary>토픽.</summary>
        public AB_Delete_Window_Components_By_Window_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Delete_Window_Components_By_Window_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>삭제된 행 수.</summary>
        public int DeletedCount;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Delete_Window_Components_By_Window_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAllLoreEntries ---

    /// <summary>활성 circuit의 lore 항목 조회 요청.</summary>
    public class AB_Get_All_Lore_Entries_Request : AB_Message
    {
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_All_Lore_Entries_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>lore 항목 조회 응답.</summary>
    public class AB_Get_All_Lore_Entries_Response : AB_Message
    {
        /// <summary>결과 lore 리스트.</summary>
        public List<AB_Lore_Entry_Model> Data = new();
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_All_Lore_Entries_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetLoreEntry (단일) ---

    /// <summary>ID로 단일 lore 조회 요청.</summary>
    public class AB_Get_Lore_Entry_Request : AB_Message
    {
        /// <summary>조회 lore ID.</summary>
        public string Id = "";
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_Lore_Entry_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>단일 lore 응답.</summary>
    public class AB_Get_Lore_Entry_Response : AB_Message
    {
        /// <summary>결과 (없으면 null).</summary>
        public AB_Lore_Entry_Model? Data;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_Lore_Entry_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- AddLoreEntry ---

    /// <summary>lore 추가 요청.</summary>
    public class AB_Add_Lore_Entry_Request : AB_Message
    {
        /// <summary>추가할 lore.</summary>
        public AB_Lore_Entry_Model Entry = new();
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Add_Lore_Entry_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>AddLoreEntry 응답.</summary>
    public class AB_Add_Lore_Entry_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Add_Lore_Entry_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- SaveLoreEntry ---

    /// <summary>lore 갱신 요청.</summary>
    public class AB_Save_Lore_Entry_Request : AB_Message
    {
        /// <summary>저장할 lore.</summary>
        public AB_Lore_Entry_Model Entry = new();
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Save_Lore_Entry_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>SaveLoreEntry 응답.</summary>
    public class AB_Save_Lore_Entry_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Save_Lore_Entry_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- DeleteLoreEntry ---

    /// <summary>lore 삭제 요청.</summary>
    public class AB_Delete_Lore_Entry_Request : AB_Message
    {
        /// <summary>삭제할 lore (Id 사용).</summary>
        public AB_Lore_Entry_Model Entry = new();
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Delete_Lore_Entry_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>DeleteLoreEntry 응답.</summary>
    public class AB_Delete_Lore_Entry_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Delete_Lore_Entry_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- FindMatchingLore ---

    /// <summary>텍스트 기반 lore 매칭 검색.</summary>
    public class AB_Find_Matching_Lore_Request : AB_Message
    {
        /// <summary>검색 대상 텍스트.</summary>
        public string Text = "";
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Find_Matching_Lore_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>FindMatchingLore 응답.</summary>
    public class AB_Find_Matching_Lore_Response : AB_Message
    {
        /// <summary>매칭된 lore 리스트.</summary>
        public List<AB_Lore_Entry_Model> Data = new();
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Find_Matching_Lore_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetSettings ---

    /// <summary>활성 circuit settings 조회 요청.</summary>
    public class AB_Get_Circuit_Settings_Request : AB_Message
    {
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_Circuit_Settings_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>circuit settings 응답 (AB_Db_Result 래핑).</summary>
    public class AB_Get_Circuit_Settings_Response : AB_Message
    {
        /// <summary>결과 settings (실패 시 null).</summary>
        public AB_Circuit_Settings_Model? Data;
        /// <summary>성공 여부.</summary>
        public bool IsOk;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_Circuit_Settings_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- SaveSettings ---

    /// <summary>활성 circuit settings 갱신 요청.</summary>
    public class AB_Save_Settings_Request : AB_Message
    {
        /// <summary>저장할 settings 인스턴스.</summary>
        public AB_Circuit_Settings_Model Settings = new();
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Save_Settings_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>SaveSettings 응답 (성공 여부 + 오류).</summary>
    public class AB_Save_Settings_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Save_Settings_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- AddSettings ---

    /// <summary>활성 circuit에 settings 신규 삽입 요청.</summary>
    public class AB_Add_Settings_Request : AB_Message
    {
        /// <summary>삽입할 settings 인스턴스.</summary>
        public AB_Circuit_Settings_Model Settings = new();
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Add_Settings_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>AddSettings 응답.</summary>
    public class AB_Add_Settings_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Add_Settings_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAllAssets (데이터 본문 포함, 무거움) ---

    /// <summary>circuit의 모든 에셋 + 데이터 본문 조회 요청 (데이터 큼).</summary>
    public class AB_Get_All_Assets_Request : AB_Message
    {
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Assets_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>전체 에셋 응답.</summary>
    public class AB_Get_All_Assets_Response : AB_Message
    {
        /// <summary>에셋 리스트 (Data_ 포함).</summary>
        public List<AB_Circuit_Asset_Model> Data = new();
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_All_Assets_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAssetData ---

    /// <summary>특정 에셋 ID의 바이너리 데이터 조회.</summary>
    public class AB_Get_Asset_Data_Request : AB_Message
    {
        /// <summary>에셋 ID.</summary>
        public string AssetId = "";
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_Asset_Data_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>에셋 바이너리 응답.</summary>
    public class AB_Get_Asset_Data_Response : AB_Message
    {
        /// <summary>바이너리 데이터 (없으면 null).</summary>
        public byte[]? Data;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_Asset_Data_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAsset (단일, ID) ---

    /// <summary>ID로 단일 에셋 조회.</summary>
    public class AB_Get_Asset_Request : AB_Message
    {
        /// <summary>에셋 ID.</summary>
        public string Id = "";
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_Asset_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>단일 에셋 응답.</summary>
    public class AB_Get_Asset_Response : AB_Message
    {
        /// <summary>결과 (없으면 null).</summary>
        public AB_Circuit_Asset_Model? Data;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_Asset_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAssetByName ---

    /// <summary>이름으로 단일 에셋 조회.</summary>
    public class AB_Get_Asset_By_Name_Request : AB_Message
    {
        /// <summary>에셋 이름.</summary>
        public string Name = "";
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_Asset_By_Name_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>이름 기반 에셋 응답.</summary>
    public class AB_Get_Asset_By_Name_Response : AB_Message
    {
        /// <summary>결과 (없으면 null).</summary>
        public AB_Circuit_Asset_Model? Data;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_Asset_By_Name_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- AddAsset / AddAssets ---

    /// <summary>단일 에셋 추가.</summary>
    public class AB_Add_Asset_Request : AB_Message
    {
        /// <summary>추가할 에셋.</summary>
        public AB_Circuit_Asset_Model Asset = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Add_Asset_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>AddAsset 응답.</summary>
    public class AB_Add_Asset_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Add_Asset_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    /// <summary>여러 에셋 일괄 추가.</summary>
    public class AB_Add_Assets_Request : AB_Message
    {
        /// <summary>추가할 에셋 리스트.</summary>
        public List<AB_Circuit_Asset_Model> Assets = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Add_Assets_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>AddAssets 응답.</summary>
    public class AB_Add_Assets_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>실제 추가된 개수.</summary>
        public int AddedCount;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Add_Assets_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- DeleteAsset ---

    /// <summary>에셋 삭제 요청.</summary>
    public class AB_Delete_Asset_Request : AB_Message
    {
        /// <summary>삭제할 에셋 (Id 사용).</summary>
        public AB_Circuit_Asset_Model Asset = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Delete_Asset_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>DeleteAsset 응답.</summary>
    public class AB_Delete_Asset_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Delete_Asset_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- FindAssetsByPathPrefix ---

    /// <summary>경로 prefix 로 에셋 검색.</summary>
    public class AB_Find_Assets_By_Path_Prefix_Request : AB_Message
    {
        /// <summary>경로 prefix.</summary>
        public string Prefix = "";
        /// <summary>토픽 자동 설정.</summary>
        public AB_Find_Assets_By_Path_Prefix_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>경로 검색 응답.</summary>
    public class AB_Find_Assets_By_Path_Prefix_Response : AB_Message
    {
        /// <summary>매칭 에셋 리스트.</summary>
        public List<AB_Circuit_Asset_Model> Data = new();
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Find_Assets_By_Path_Prefix_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAllAssetMetadata ---

    /// <summary>활성 circuit 에셋 메타데이터(데이터 본문 제외) 조회 요청.</summary>
    public class AB_Get_All_Asset_Metadata_Request : AB_Message
    {
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_All_Asset_Metadata_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>에셋 메타데이터 응답.</summary>
    public class AB_Get_All_Asset_Metadata_Response : AB_Message
    {
        /// <summary>결과 에셋 리스트 (Data_ 컬럼 제외).</summary>
        public List<AB_Circuit_Asset_Model> Data = new();
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_All_Asset_Metadata_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetAllUiTemplates ---

    /// <summary>활성 circuit의 UI 템플릿 목록 조회 요청.</summary>
    public class AB_Get_All_Ui_Templates_Request : AB_Message
    {
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_All_Ui_Templates_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>UI 템플릿 목록 응답.</summary>
    public class AB_Get_All_Ui_Templates_Response : AB_Message
    {
        /// <summary>결과 템플릿 리스트.</summary>
        public List<AB_Circuit_Ui_Template_Model> Data = new();
        /// <summary>오류 메시지.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정 생성자.</summary>
        public AB_Get_All_Ui_Templates_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- AddUiTemplate ---

    /// <summary>UI 템플릿 추가 요청.</summary>
    public class AB_Add_Ui_Template_Request : AB_Message
    {
        /// <summary>대상 템플릿.</summary>
        public AB_Circuit_Ui_Template_Model Template = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Add_Ui_Template_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>UI 템플릿 추가 응답.</summary>
    public class AB_Add_Ui_Template_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Add_Ui_Template_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- SaveUiTemplate ---

    /// <summary>UI 템플릿 저장 요청.</summary>
    public class AB_Save_Ui_Template_Request : AB_Message
    {
        /// <summary>대상 템플릿.</summary>
        public AB_Circuit_Ui_Template_Model Template = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Save_Ui_Template_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>UI 템플릿 저장 응답.</summary>
    public class AB_Save_Ui_Template_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Save_Ui_Template_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- DeleteUiTemplate ---

    /// <summary>UI 템플릿 삭제 요청.</summary>
    public class AB_Delete_Ui_Template_Request : AB_Message
    {
        /// <summary>대상 템플릿.</summary>
        public AB_Circuit_Ui_Template_Model Template = new();
        /// <summary>토픽 자동 설정.</summary>
        public AB_Delete_Ui_Template_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>UI 템플릿 삭제 응답.</summary>
    public class AB_Delete_Ui_Template_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Delete_Ui_Template_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- SetActiveUiTemplate ---

    /// <summary>활성 UI 템플릿 설정 요청.</summary>
    public class AB_Set_Active_Ui_Template_Request : AB_Message
    {
        /// <summary>활성화할 템플릿 ID.</summary>
        public string TemplateId = "";
        /// <summary>토픽 자동 설정.</summary>
        public AB_Set_Active_Ui_Template_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>활성 UI 템플릿 설정 응답.</summary>
    public class AB_Set_Active_Ui_Template_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Set_Active_Ui_Template_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetActiveUiTemplate ---

    /// <summary>현재 활성 UI 템플릿 조회 요청.</summary>
    public class AB_Get_Active_Ui_Template_Request : AB_Message
    {
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_Active_Ui_Template_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }

    /// <summary>활성 UI 템플릿 조회 응답.</summary>
    public class AB_Get_Active_Ui_Template_Response : AB_Message
    {
        /// <summary>활성 템플릿 (없으면 null).</summary>
        public AB_Circuit_Ui_Template_Model? Data;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽 자동 설정.</summary>
        public AB_Get_Active_Ui_Template_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // ====================================================================
    // H. Vec — 임베딩/벡터 검색 (Phase B 카테고리 H)
    //   AB_Vec_Hit / AB_Vec_Chat_Hit / AB_Chat_Embedding_Info 는 Common/Requests/AB_Vec_Hit_Types.cs
    //   (Circuit / Persona 양쪽 Requests 가 공유).
    // ====================================================================

    // --- IsVecInitialized ---

    /// <summary>벡터 저장소 초기화 여부 조회.</summary>
    public class AB_Is_Vec_Initialized_Request : AB_Message
    {
        /// <summary>토픽.</summary>
        public AB_Is_Vec_Initialized_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Is_Vec_Initialized_Response : AB_Message
    {
        /// <summary>초기화 여부.</summary>
        public bool Initialized;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Is_Vec_Initialized_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetVecTotalRowCount ---

    /// <summary>vec 전체 행 수 조회.</summary>
    public class AB_Get_Vec_Total_Row_Count_Request : AB_Message
    {
        /// <summary>토픽.</summary>
        public AB_Get_Vec_Total_Row_Count_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Get_Vec_Total_Row_Count_Response : AB_Message
    {
        /// <summary>행 수.</summary>
        public int Count;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Get_Vec_Total_Row_Count_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- ClearAllVec ---

    /// <summary>vec 전체 데이터 삭제.</summary>
    public class AB_Clear_All_Vec_Request : AB_Message
    {
        /// <summary>토픽.</summary>
        public AB_Clear_All_Vec_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Clear_All_Vec_Response : AB_Message
    {
        /// <summary>성공 여부.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Clear_All_Vec_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- SearchLore ---

    /// <summary>로어 임베딩 유사도 검색.</summary>
    public class AB_Search_Lore_Vec_Request : AB_Message
    {
        /// <summary>쿼리 벡터.</summary>
        public float[] Query = System.Array.Empty<float>();
        /// <summary>top-K.</summary>
        public int TopK;
        /// <summary>토픽.</summary>
        public AB_Search_Lore_Vec_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Search_Lore_Vec_Response : AB_Message
    {
        /// <summary>결과 (loreId + 거리).</summary>
        public List<AB_Vec_Hit> Hits = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Search_Lore_Vec_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- SearchChat ---

    /// <summary>채팅 임베딩 유사도 검색.</summary>
    public class AB_Search_Chat_Vec_Request : AB_Message
    {
        /// <summary>쿼리 벡터.</summary>
        public float[] Query = System.Array.Empty<float>();
        /// <summary>top-K.</summary>
        public int TopK;
        /// <summary>제외할 세션 ID (null이면 전체).</summary>
        public string? ExcludeSessionId;
        /// <summary>토픽.</summary>
        public AB_Search_Chat_Vec_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Search_Chat_Vec_Response : AB_Message
    {
        /// <summary>결과.</summary>
        public List<AB_Vec_Chat_Hit> Hits = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Search_Chat_Vec_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- SearchCData ---

    /// <summary>CData 임베딩 유사도 검색.</summary>
    public class AB_Search_CData_Vec_Request : AB_Message
    {
        /// <summary>쿼리 벡터.</summary>
        public float[] Query = System.Array.Empty<float>();
        /// <summary>top-K.</summary>
        public int TopK = 10;
        /// <summary>토픽.</summary>
        public AB_Search_CData_Vec_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Search_CData_Vec_Response : AB_Message
    {
        /// <summary>결과.</summary>
        public List<AB_Vec_Hit> Hits = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Search_CData_Vec_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- InsertLoreEmbedding ---

    /// <summary>로어 임베딩 삽입.</summary>
    public class AB_Insert_Lore_Embedding_Request : AB_Message
    {
        /// <summary>로어 ID.</summary>
        public string LoreId = "";
        /// <summary>벡터.</summary>
        public float[] Embedding = System.Array.Empty<float>();
        /// <summary>토픽.</summary>
        public AB_Insert_Lore_Embedding_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Insert_Lore_Embedding_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Insert_Lore_Embedding_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- DeleteLoreEmbedding ---

    /// <summary>로어 임베딩 삭제.</summary>
    public class AB_Delete_Lore_Embedding_Request : AB_Message
    {
        /// <summary>로어 ID.</summary>
        public string LoreId = "";
        /// <summary>토픽.</summary>
        public AB_Delete_Lore_Embedding_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Delete_Lore_Embedding_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Delete_Lore_Embedding_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- InsertChatEmbedding ---

    /// <summary>채팅 임베딩 삽입. 키 튜플은 context_records 와 동일.</summary>
    public class AB_Insert_Chat_Embedding_Request : AB_Message
    {
        /// <summary>세션 ID.</summary>
        public string SessionId = "";
        /// <summary>소스 노드 ID.</summary>
        public string NodeId = "";
        /// <summary>턴 인덱스.</summary>
        public int TurnIndex;
        /// <summary>refresh 레이어 인덱스.</summary>
        public int RefreshIndex;
        /// <summary>발행 순서.</summary>
        public int EmissionOrder;
        /// <summary>벡터.</summary>
        public float[] Embedding = System.Array.Empty<float>();
        /// <summary>토픽.</summary>
        public AB_Insert_Chat_Embedding_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Insert_Chat_Embedding_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Insert_Chat_Embedding_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- DeleteChatEmbeddingsBySession ---

    /// <summary>세션 채팅 임베딩 일괄 삭제.</summary>
    public class AB_Delete_Chat_Embeddings_By_Session_Request : AB_Message
    {
        /// <summary>세션 ID.</summary>
        public string SessionId = "";
        /// <summary>토픽.</summary>
        public AB_Delete_Chat_Embeddings_By_Session_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Delete_Chat_Embeddings_By_Session_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Delete_Chat_Embeddings_By_Session_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- DeleteChatEmbeddingByRecord ---

    /// <summary>특정 컨텍스트 레코드의 채팅 임베딩 삭제.</summary>
    public class AB_Delete_Chat_Embedding_By_Record_Request : AB_Message
    {
        /// <summary>세션 ID.</summary>
        public string SessionId = "";
        /// <summary>소스 노드 ID.</summary>
        public string NodeId = "";
        /// <summary>턴 인덱스.</summary>
        public int TurnIndex;
        /// <summary>refresh 레이어 인덱스.</summary>
        public int RefreshIndex;
        /// <summary>발행 순서.</summary>
        public int EmissionOrder;
        /// <summary>토픽.</summary>
        public AB_Delete_Chat_Embedding_By_Record_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Delete_Chat_Embedding_By_Record_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Delete_Chat_Embedding_By_Record_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- GetChatEmbeddingsBySession ---

    /// <summary>세션 임베딩 메타 목록.</summary>
    public class AB_Get_Chat_Embeddings_By_Session_Request : AB_Message
    {
        /// <summary>세션 ID.</summary>
        public string SessionId = "";
        /// <summary>토픽.</summary>
        public AB_Get_Chat_Embeddings_By_Session_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Get_Chat_Embeddings_By_Session_Response : AB_Message
    {
        /// <summary>결과 메타 리스트.</summary>
        public List<AB_Chat_Embedding_Info> Data = new();
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Get_Chat_Embeddings_By_Session_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- InsertCDataEmbedding ---

    /// <summary>CData 임베딩 삽입.</summary>
    public class AB_Insert_CData_Embedding_Request : AB_Message
    {
        /// <summary>CData ID.</summary>
        public string CDataId = "";
        /// <summary>벡터.</summary>
        public float[] Embedding = System.Array.Empty<float>();
        /// <summary>토픽.</summary>
        public AB_Insert_CData_Embedding_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Insert_CData_Embedding_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Insert_CData_Embedding_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- OpenVecFile ---

    /// <summary>Vec(.vec) 파일 열기.</summary>
    public class AB_Open_Vec_File_Request : AB_Message
    {
        /// <summary>토픽.</summary>
        public AB_Open_Vec_File_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Open_Vec_File_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Open_Vec_File_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- RenameVecFile ---

    /// <summary>Vec 파일 이름 변경.</summary>
    public class AB_Rename_Vec_File_Request : AB_Message
    {
        /// <summary>이전 이름.</summary>
        public string OldName = "";
        /// <summary>새 이름.</summary>
        public string NewName = "";
        /// <summary>토픽.</summary>
        public AB_Rename_Vec_File_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    /// <summary>응답.</summary>
    public class AB_Rename_Vec_File_Response : AB_Message
    {
        /// <summary>성공.</summary>
        public bool Success;
        /// <summary>오류.</summary>
        public string? Error;
        /// <summary>토픽.</summary>
        public AB_Rename_Vec_File_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }

    // --- DeleteCircuitFile (DB 매니저 단일 창구로 .circuit + .vec 동반 삭제. 활성 핸들이면 close 우선) ---

    /// <summary>Circuit 파일 삭제 요청 — Gateway 가 핸들 close 후 .circuit + .vec 정리. [[db-access]] 준수.</summary>
    public class AB_Delete_Circuit_File_Request : AB_Message
    {
        public string Name = "";
        public AB_Delete_Circuit_File_Request() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; }
    }
    public class AB_Delete_Circuit_File_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Circuit_File_Response() { Topic = AB_Circuit_Db_Topics.ActiveCircuit; IsResponse = true; }
    }
}
