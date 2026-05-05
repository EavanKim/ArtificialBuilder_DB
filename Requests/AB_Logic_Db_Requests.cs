using ArtificialBuilder.Models;
using ArtificialBuilder_EDP.Core.Messaging;
using System.Collections.Generic;

namespace ArtificialBuilder.Requests
{
    /// <summary>Logic DB 게이트웨이가 처리하는 메시지 토픽 상수 ([[app-logic-separation]] — 노드 정보만).</summary>
    public static class AB_Logic_Db_Topics
    {
        /// <summary>현재 활성 Logic DB 에 대한 모든 요청.</summary>
        public const string ActiveLogic = "db.logic.active";
    }

    // --- Meta ---

    /// <summary>활성 Logic 메타 조회 요청.</summary>
    public class AB_Get_Logic_Meta_Request : AB_Message
    {
        public AB_Get_Logic_Meta_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    /// <summary>Logic 메타 조회 응답.</summary>
    public class AB_Get_Logic_Meta_Response : AB_Message
    {
        public AB_Logic_Meta_Model? Data;
        public string? Error;
        public AB_Get_Logic_Meta_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    /// <summary>Logic 메타 갱신 요청.</summary>
    public class AB_Save_Logic_Meta_Request : AB_Message
    {
        public AB_Logic_Meta_Model Meta = new();
        public AB_Save_Logic_Meta_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    /// <summary>Logic 메타 갱신 응답.</summary>
    public class AB_Save_Logic_Meta_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Save_Logic_Meta_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    // --- Used Circuits ---

    /// <summary>활성 Logic 의 사용 서킷 list 조회.</summary>
    public class AB_Get_All_Logic_Used_Circuits_Request : AB_Message
    {
        public AB_Get_All_Logic_Used_Circuits_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Get_All_Logic_Used_Circuits_Response : AB_Message
    {
        public List<AB_Logic_Used_Circuit_Model> Data = new();
        public string? Error;
        public AB_Get_All_Logic_Used_Circuits_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    public class AB_Add_Logic_Used_Circuit_Request : AB_Message
    {
        public AB_Logic_Used_Circuit_Model Item = new();
        public AB_Add_Logic_Used_Circuit_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Add_Logic_Used_Circuit_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Add_Logic_Used_Circuit_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    public class AB_Save_Logic_Used_Circuit_Request : AB_Message
    {
        public AB_Logic_Used_Circuit_Model Item = new();
        public AB_Save_Logic_Used_Circuit_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Save_Logic_Used_Circuit_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Save_Logic_Used_Circuit_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    public class AB_Delete_Logic_Used_Circuit_Request : AB_Message
    {
        public AB_Logic_Used_Circuit_Model Item = new();
        public AB_Delete_Logic_Used_Circuit_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Delete_Logic_Used_Circuit_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Logic_Used_Circuit_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    // --- Used Response UI ---

    public class AB_Get_All_Logic_Used_Response_Ui_Request : AB_Message
    {
        public AB_Get_All_Logic_Used_Response_Ui_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Get_All_Logic_Used_Response_Ui_Response : AB_Message
    {
        public List<AB_Logic_Used_Response_Ui_Model> Data = new();
        public string? Error;
        public AB_Get_All_Logic_Used_Response_Ui_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    public class AB_Add_Logic_Used_Response_Ui_Request : AB_Message
    {
        public AB_Logic_Used_Response_Ui_Model Item = new();
        public AB_Add_Logic_Used_Response_Ui_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Add_Logic_Used_Response_Ui_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Add_Logic_Used_Response_Ui_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    public class AB_Delete_Logic_Used_Response_Ui_Request : AB_Message
    {
        public AB_Logic_Used_Response_Ui_Model Item = new();
        public AB_Delete_Logic_Used_Response_Ui_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Delete_Logic_Used_Response_Ui_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Logic_Used_Response_Ui_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    // --- Sub Logics ---

    public class AB_Get_All_Logic_Sub_Logics_Request : AB_Message
    {
        public AB_Get_All_Logic_Sub_Logics_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Get_All_Logic_Sub_Logics_Response : AB_Message
    {
        public List<AB_Logic_Sub_Logic_Model> Data = new();
        public string? Error;
        public AB_Get_All_Logic_Sub_Logics_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    public class AB_Add_Logic_Sub_Logic_Request : AB_Message
    {
        public AB_Logic_Sub_Logic_Model Item = new();
        public AB_Add_Logic_Sub_Logic_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Add_Logic_Sub_Logic_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Add_Logic_Sub_Logic_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    public class AB_Delete_Logic_Sub_Logic_Request : AB_Message
    {
        public AB_Logic_Sub_Logic_Model Item = new();
        public AB_Delete_Logic_Sub_Logic_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Delete_Logic_Sub_Logic_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Logic_Sub_Logic_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    // --- History Turns ---

    public class AB_Get_All_Logic_History_Turns_Request : AB_Message
    {
        public AB_Get_All_Logic_History_Turns_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Get_All_Logic_History_Turns_Response : AB_Message
    {
        public List<AB_Logic_History_Turn_Model> Data = new();
        public string? Error;
        public AB_Get_All_Logic_History_Turns_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }

    public class AB_Append_Logic_History_Turn_Request : AB_Message
    {
        public AB_Logic_History_Turn_Model Item = new();
        public AB_Append_Logic_History_Turn_Request() { Topic = AB_Logic_Db_Topics.ActiveLogic; }
    }

    public class AB_Append_Logic_History_Turn_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Append_Logic_History_Turn_Response() { Topic = AB_Logic_Db_Topics.ActiveLogic; IsResponse = true; }
    }
}
