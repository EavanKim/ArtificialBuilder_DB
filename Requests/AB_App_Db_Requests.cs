using ArtificialBuilder.Models;
using ArtificialBuilder_EDP.Core.Messaging;
using System.Collections.Generic;

// Phase E — wire-safety 감사 (2026-04-16)
// - POCO 필드는 JSON 직렬화 가능 타입만 사용. object?/인터페이스/delegate 없음. OK.
// - 대용량 payload 없음 (App DB 는 ModelConfig / UiTemplate / Pipeline 메타).
// - AB_In_Memory_Broker 는 객체참조 전달, Redis 교체 시에도 payload 이슈 없음.
namespace ArtificialBuilder.Requests
{
    /// <summary>
    /// App DB 게이트웨이 토픽. 단일 전역 ArtificialBuilder.db 대상.
    /// </summary>
    public static class AB_App_Db_Topics
    {
        /// <summary>전역 App DB 모든 요청.</summary>
        public const string App = "db.app";
    }

    // ============================================================
    // ModelConfig
    // ============================================================

    /// <summary>모델 설정 전체 조회.</summary>
    public class AB_Get_All_Models_Request : AB_Message
    {
        public AB_Get_All_Models_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>모델 설정 전체 응답.</summary>
    public class AB_Get_All_Models_Response : AB_Message
    {
        public List<AB_Model_Config_Model> Data = new();
        public string? Error;
        public AB_Get_All_Models_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>ID로 모델 단건 조회. (ddo-datakey-typed sub 2) Id string → long 마이그 — AB_Model_Config_Model.Id_ 본체 long 정합.</summary>
    public class AB_Get_Model_By_Id_Request : AB_Message
    {
        public long Id;
        public AB_Get_Model_By_Id_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>모델 단건 응답.</summary>
    public class AB_Get_Model_By_Id_Response : AB_Message
    {
        public AB_Model_Config_Model? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_Model_By_Id_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>모델 추가.</summary>
    public class AB_Add_Model_Request : AB_Message
    {
        public AB_Model_Config_Model Model = new();
        public AB_Add_Model_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>모델 추가 응답.</summary>
    public class AB_Add_Model_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Add_Model_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>모델 갱신.</summary>
    public class AB_Update_Model_Request : AB_Message
    {
        public AB_Model_Config_Model Model = new();
        public AB_Update_Model_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>모델 갱신 응답.</summary>
    public class AB_Update_Model_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Update_Model_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>모델 삭제.</summary>
    public class AB_Delete_Model_Request : AB_Message
    {
        public AB_Model_Config_Model Model = new();
        public AB_Delete_Model_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>모델 삭제 응답.</summary>
    public class AB_Delete_Model_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Model_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    // ============================================================
    // UiTemplate (글로벌)
    // ============================================================

    /// <summary>전역 UI 템플릿 전체 조회.</summary>
    public class AB_Get_All_App_Ui_Templates_Request : AB_Message
    {
        public AB_Get_All_App_Ui_Templates_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>전역 UI 템플릿 응답.</summary>
    public class AB_Get_All_App_Ui_Templates_Response : AB_Message
    {
        public List<AB_Ui_Template_Model> Data = new();
        public string? Error;
        public AB_Get_All_App_Ui_Templates_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>ID로 글로벌 UI 템플릿 단건.</summary>
    public class AB_Get_App_Ui_Template_By_Id_Request : AB_Message
    {
        public string Id = "";
        public AB_Get_App_Ui_Template_By_Id_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>글로벌 UI 템플릿 단건 응답.</summary>
    public class AB_Get_App_Ui_Template_By_Id_Response : AB_Message
    {
        public AB_Ui_Template_Model? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_App_Ui_Template_By_Id_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>글로벌 UI 템플릿 추가.</summary>
    public class AB_Add_App_Ui_Template_Request : AB_Message
    {
        public AB_Ui_Template_Model Template = new();
        public AB_Add_App_Ui_Template_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>글로벌 UI 템플릿 추가 응답.</summary>
    public class AB_Add_App_Ui_Template_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Add_App_Ui_Template_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>글로벌 UI 템플릿 갱신.</summary>
    public class AB_Update_App_Ui_Template_Request : AB_Message
    {
        public AB_Ui_Template_Model Template = new();
        public AB_Update_App_Ui_Template_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>글로벌 UI 템플릿 갱신 응답.</summary>
    public class AB_Update_App_Ui_Template_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Update_App_Ui_Template_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>글로벌 UI 템플릿 삭제.</summary>
    public class AB_Delete_App_Ui_Template_Request : AB_Message
    {
        public AB_Ui_Template_Model Template = new();
        public AB_Delete_App_Ui_Template_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>글로벌 UI 템플릿 삭제 응답.</summary>
    public class AB_Delete_App_Ui_Template_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_App_Ui_Template_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    // ============================================================
    // Pipeline (레거시)
    // ============================================================

    /// <summary>파이프라인 전체 조회.</summary>
    public class AB_Get_All_Pipelines_Request : AB_Message
    {
        public AB_Get_All_Pipelines_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>파이프라인 전체 응답.</summary>
    public class AB_Get_All_Pipelines_Response : AB_Message
    {
        public List<AB_Pipeline_Model> Data = new();
        public string? Error;
        public AB_Get_All_Pipelines_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>ID로 파이프라인 단건.</summary>
    public class AB_Get_Pipeline_By_Id_Request : AB_Message
    {
        public string Id = "";
        public AB_Get_Pipeline_By_Id_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>파이프라인 단건 응답.</summary>
    public class AB_Get_Pipeline_By_Id_Response : AB_Message
    {
        public AB_Pipeline_Model? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_Pipeline_By_Id_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>파이프라인 추가.</summary>
    public class AB_Add_Pipeline_Request : AB_Message
    {
        public AB_Pipeline_Model Pipeline = new();
        public AB_Add_Pipeline_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>파이프라인 추가 응답.</summary>
    public class AB_Add_Pipeline_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Add_Pipeline_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>파이프라인 갱신.</summary>
    public class AB_Update_Pipeline_Request : AB_Message
    {
        public AB_Pipeline_Model Pipeline = new();
        public AB_Update_Pipeline_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>파이프라인 갱신 응답.</summary>
    public class AB_Update_Pipeline_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Update_Pipeline_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>파이프라인 삭제.</summary>
    public class AB_Delete_Pipeline_Request : AB_Message
    {
        public AB_Pipeline_Model Pipeline = new();
        public AB_Delete_Pipeline_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>파이프라인 삭제 응답.</summary>
    public class AB_Delete_Pipeline_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Pipeline_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    // ============================================================
    // Llama_Model (typed-id-edp-rebase chunk 4o)
    // 로컬 GGUF 모델 entity. PK = long (AB_Model_Id 매핑). filename = entity attribute.
    // ============================================================

    /// <summary>Llama 모델 전체 조회.</summary>
    public class AB_Get_All_Llama_Models_Request : AB_Message
    {
        public AB_Get_All_Llama_Models_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>Llama 모델 전체 응답.</summary>
    public class AB_Get_All_Llama_Models_Response : AB_Message
    {
        public List<AB_Llama_Model> Data = new();
        public string? Error;
        public AB_Get_All_Llama_Models_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>ID로 Llama 모델 단건.</summary>
    public class AB_Get_Llama_Model_By_Id_Request : AB_Message
    {
        public long Id;
        public AB_Get_Llama_Model_By_Id_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>Llama 모델 단건 응답.</summary>
    public class AB_Get_Llama_Model_By_Id_Response : AB_Message
    {
        public AB_Llama_Model? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_Llama_Model_By_Id_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>FileName 으로 Llama 모델 단건 조회 (scan-on-load upsert 용).</summary>
    public class AB_Get_Llama_Model_By_File_Name_Request : AB_Message
    {
        public string FileName = "";
        public AB_Get_Llama_Model_By_File_Name_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>FileName 으로 Llama 모델 단건 응답.</summary>
    public class AB_Get_Llama_Model_By_File_Name_Response : AB_Message
    {
        public AB_Llama_Model? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_Llama_Model_By_File_Name_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>Llama 모델 upsert (filename unique key — 존재 시 update, 없으면 add).</summary>
    public class AB_Upsert_Llama_Model_Request : AB_Message
    {
        public AB_Llama_Model Model = new();
        public AB_Upsert_Llama_Model_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>Llama 모델 upsert 응답. Data = 저장된 row (Id 포함).</summary>
    public class AB_Upsert_Llama_Model_Response : AB_Message
    {
        public AB_Llama_Model? Data;
        public bool Success;
        public string? Error;
        public AB_Upsert_Llama_Model_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>Llama 모델 삭제 (Id 기반).</summary>
    public class AB_Delete_Llama_Model_Request : AB_Message
    {
        public long Id;
        public AB_Delete_Llama_Model_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>Llama 모델 삭제 응답.</summary>
    public class AB_Delete_Llama_Model_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_Llama_Model_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    // ============================================================
    // HF_Download (typed-id-edp-rebase chunk 4p)
    // ============================================================

    /// <summary>HF 다운로드 전체 조회.</summary>
    public class AB_Get_All_HF_Downloads_Request : AB_Message
    {
        public AB_Get_All_HF_Downloads_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>HF 다운로드 전체 응답.</summary>
    public class AB_Get_All_HF_Downloads_Response : AB_Message
    {
        public List<AB_HF_Download> Data = new();
        public string? Error;
        public AB_Get_All_HF_Downloads_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>Id 로 HF 다운로드 단건.</summary>
    public class AB_Get_HF_Download_By_Id_Request : AB_Message
    {
        public long Id;
        public AB_Get_HF_Download_By_Id_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>HF 다운로드 단건 응답.</summary>
    public class AB_Get_HF_Download_By_Id_Response : AB_Message
    {
        public AB_HF_Download? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_HF_Download_By_Id_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>HF 다운로드 추가 (Enqueue 시점 — long Id 발급). 반환 = 저장된 row.</summary>
    public class AB_Add_HF_Download_Request : AB_Message
    {
        public AB_HF_Download Model = new();
        public AB_Add_HF_Download_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>HF 다운로드 추가 응답.</summary>
    public class AB_Add_HF_Download_Response : AB_Message
    {
        public AB_HF_Download? Data;
        public bool Success;
        public string? Error;
        public AB_Add_HF_Download_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>HF 다운로드 삭제 (Id 기반).</summary>
    public class AB_Delete_HF_Download_Request : AB_Message
    {
        public long Id;
        public AB_Delete_HF_Download_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>HF 다운로드 삭제 응답.</summary>
    public class AB_Delete_HF_Download_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_HF_Download_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    // ============================================================
    // HF_Repo (typed-id-edp-rebase chunk 4q)
    // ============================================================

    /// <summary>HF repo 전체 조회.</summary>
    public class AB_Get_All_HF_Repos_Request : AB_Message
    {
        public AB_Get_All_HF_Repos_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>HF repo 전체 응답.</summary>
    public class AB_Get_All_HF_Repos_Response : AB_Message
    {
        public List<AB_HF_Repo> Data = new();
        public string? Error;
        public AB_Get_All_HF_Repos_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>Id 로 HF repo 단건.</summary>
    public class AB_Get_HF_Repo_By_Id_Request : AB_Message
    {
        public long Id;
        public AB_Get_HF_Repo_By_Id_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>HF repo 단건 응답.</summary>
    public class AB_Get_HF_Repo_By_Id_Response : AB_Message
    {
        public AB_HF_Repo? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_HF_Repo_By_Id_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>RepoId ("owner/repo") 로 HF repo 단건 조회 (upsert 용).</summary>
    public class AB_Get_HF_Repo_By_Repo_Id_Request : AB_Message
    {
        public string RepoId = "";
        public AB_Get_HF_Repo_By_Repo_Id_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>RepoId 로 HF repo 단건 응답.</summary>
    public class AB_Get_HF_Repo_By_Repo_Id_Response : AB_Message
    {
        public AB_HF_Repo? Data;
        public bool IsOk;
        public string? Error;
        public AB_Get_HF_Repo_By_Repo_Id_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>HF repo upsert (repo_id unique). 반환 = 저장된 row (Id 포함).</summary>
    public class AB_Upsert_HF_Repo_Request : AB_Message
    {
        public AB_HF_Repo Model = new();
        public AB_Upsert_HF_Repo_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>HF repo upsert 응답.</summary>
    public class AB_Upsert_HF_Repo_Response : AB_Message
    {
        public AB_HF_Repo? Data;
        public bool Success;
        public string? Error;
        public AB_Upsert_HF_Repo_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }

    /// <summary>HF repo 삭제 (Id 기반).</summary>
    public class AB_Delete_HF_Repo_Request : AB_Message
    {
        public long Id;
        public AB_Delete_HF_Repo_Request() { Topic = AB_App_Db_Topics.App; }
    }

    /// <summary>HF repo 삭제 응답.</summary>
    public class AB_Delete_HF_Repo_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Delete_HF_Repo_Response() { Topic = AB_App_Db_Topics.App; IsResponse = true; }
    }
}
