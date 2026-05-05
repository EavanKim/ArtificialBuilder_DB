using ArtificialBuilder.Models;
using System.Collections.Generic;

namespace ArtificialBuilder
{
    // --- 페르소나 ---

    /// <summary>페르소나 목록 변경 이벤트.</summary>
    public struct Persona_List_Changed
    {
        /// <summary>전체 페르소나 이름 목록.</summary>
        public List<string> Names_;
        /// <summary>현재 활성 페르소나 이름.</summary>
        public string? ActiveName_;
    }

    /// <summary>페르소나 전환 이벤트.</summary>
    public struct Persona_Switched
    {
        /// <summary>전환된 페르소나 이름.</summary>
        public string Name_;
    }

    // --- Circuit ---
    // CircuitListChanged / CircuitOpened / CircuitSettingsChanged — Phase F (2026-04-16) 제거.
    // 구독자 0 확인 후 삭제. Circuit 수명 변경 통지는 AB_Circuit_Registry.CircuitSwapped / CircuitInvalidated 로 일원화.

    // --- 채팅 세션 ---

    /// <summary>채팅 세션 목록 변경 이벤트.</summary>
    public struct Chat_List_Changed
    {
        /// <summary>세션 목록 (현재 미사용 - 수신자 재조회 방식).</summary>
        public List<AB_Chat_Session_Model> Sessions_;
    }

    /// <summary>채팅 세션 열림 이벤트.</summary>
    public struct Chat_Opened
    {
        /// <summary>열린 세션 ID.</summary>
        public string SessionId_;
        /// <summary>세션의 메시지 목록.</summary>
        public List<AB_Chat_Message_Model> Messages_;
    }

    /// <summary>채팅 세션 닫힘 이벤트. 다른 세션으로 전환되거나 명시적으로 닫힐 때 발행.</summary>
    public struct Chat_Closed
    {
        /// <summary>닫힌 세션 ID.</summary>
        public string SessionId_;
    }

    /// <summary>채팅 메시지 수신 이벤트 (현재 미사용 - AB_Chat 콜백 사용).</summary>
    public struct Chat_Message_Received
    {
        /// <summary>수신된 메시지.</summary>
        public AB_Chat_Message_Model Message_;
    }

    /// <summary>채팅 세션 이름 변경 이벤트.</summary>
    public struct Chat_Session_Renamed
    {
        /// <summary>변경된 세션 ID.</summary>
        public string SessionId_;
        /// <summary>새 제목.</summary>
        public string NewTitle_;
    }

    // --- 캐릭터 ---

    /// <summary>캐릭터 목록 변경 이벤트.</summary>
    public struct Character_List_Changed
    {
        /// <summary>전체 캐릭터 목록.</summary>
        public List<AB_Character_Model> Characters_;
    }

    // --- 모델 ---

    /// <summary>모델 목록 변경 이벤트.</summary>
    public struct Model_List_Changed
    {
        /// <summary>전체 모델 목록.</summary>
        public List<AB_Model_Config_Model> Models_;
    }

    // --- 템플릿 ---

    /// <summary>템플릿 목록 변경 이벤트.</summary>
    public struct Template_List_Changed
    {
        /// <summary>전체 템플릿 목록 (현재 미사용 - 수신자 재조회 방식).</summary>
        public List<AB_Response_Ui_Template_Model> Templates_;
    }

    // --- 로어 ---

    /// <summary>로어 목록 변경 이벤트.</summary>
    public struct Lore_List_Changed
    {
        /// <summary>전체 로어 항목 목록.</summary>
        public List<AB_Lore_Entry_Model> Entries_;
    }

    // --- 장소 ---

    /// <summary>장소 목록 변경 이벤트 (현재 미사용).</summary>
    public struct Location_List_Changed
    {
        /// <summary>전체 장소 목록.</summary>
        public List<AB_Location_Model> Locations_;
    }

    // --- 관계 ---

    /// <summary>관계 목록 변경 이벤트 (현재 미사용).</summary>
    public struct Relationship_List_Changed
    {
        /// <summary>전체 관계 목록.</summary>
        public List<AB_Character_Relationship_Model> Relationships_;
    }
}
