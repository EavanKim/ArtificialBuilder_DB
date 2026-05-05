using ArtificialBuilder.Models;
using ArtificialBuilder_EDP.Core.Messaging;
using System.Collections.Generic;

// 주의: Window / Component / Template 관련 request 는 OLD 명칭 그대로 AB_Circuit_Db_Requests.cs 에 보존 (Topic 만 본 ActiveResponseUi 로 redirect). 본 파일 = NEW (Layer / Meta) 영역.
namespace ArtificialBuilder.Requests
{
    /// <summary>Response UI DB 게이트웨이 토픽 ([[app-logic-separation]] — 화면 구성).</summary>
    public static class AB_Response_Ui_Db_Topics
    {
        public const string ActiveResponseUi = "db.response_ui.active";
    }

    // --- Meta ---

    public class AB_Get_Response_Ui_Meta_Request : AB_Message
    {
        public AB_Get_Response_Ui_Meta_Request() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; }
    }

    public class AB_Get_Response_Ui_Meta_Response : AB_Message
    {
        public AB_Response_Ui_Meta_Model? Data;
        public string? Error;
        public AB_Get_Response_Ui_Meta_Response() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; IsResponse = true; }
    }

    public class AB_Save_Response_Ui_Meta_Request : AB_Message
    {
        public AB_Response_Ui_Meta_Model Meta = new();
        public AB_Save_Response_Ui_Meta_Request() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; }
    }

    public class AB_Save_Response_Ui_Meta_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Save_Response_Ui_Meta_Response() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; IsResponse = true; }
    }

    // --- Layers ---

    public class AB_Get_All_Response_Ui_Layers_Request : AB_Message
    {
        public AB_Get_All_Response_Ui_Layers_Request() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; }
    }

    public class AB_Get_All_Response_Ui_Layers_Response : AB_Message
    {
        public List<AB_Response_Ui_Layer_Model> Data = new();
        public string? Error;
        public AB_Get_All_Response_Ui_Layers_Response() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; IsResponse = true; }
    }

    public class AB_Add_Response_Ui_Layer_Request : AB_Message
    {
        public AB_Response_Ui_Layer_Model Item = new();
        public AB_Add_Response_Ui_Layer_Request() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; }
    }

    public class AB_Add_Response_Ui_Layer_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Add_Response_Ui_Layer_Response() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; IsResponse = true; }
    }

    public class AB_Save_Response_Ui_Layer_Request : AB_Message
    {
        public AB_Response_Ui_Layer_Model Item = new();
        public AB_Save_Response_Ui_Layer_Request() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; }
    }

    public class AB_Save_Response_Ui_Layer_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Save_Response_Ui_Layer_Response() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; IsResponse = true; }
    }

    public class AB_Delete_Response_Ui_Layer_Request : AB_Message
    {
        public AB_Response_Ui_Layer_Model Item = new();
        public AB_Delete_Response_Ui_Layer_Request() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; }
    }

    public class AB_Delete_Response_Ui_Layer_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Response_Ui_Layer_Response() { Topic = AB_Response_Ui_Db_Topics.ActiveResponseUi; IsResponse = true; }
    }
}
