using ArtificialBuilder.Models;
using ArtificialBuilder_EDP.Core.Messaging;
using System.Collections.Generic;

// Phase E — wire-safety 감사 (2026-04-16)
// - POCO 필드는 JSON 직렬화 가능 타입만 사용. object?/인터페이스/delegate 없음. OK.
// - 대용량 payload 없음 (진단 로그 엔트리 텍스트).
namespace ArtificialBuilder.Requests
{
    /// <summary>
    /// 파이프라인 디버그 DB 게이트웨이 토픽. 단일 pipeline_debug.pdb 대상.
    /// </summary>
    public static class AB_Pipeline_Debug_Db_Topics
    {
        /// <summary>파이프라인 디버그 DB 모든 요청.</summary>
        public const string PipelineDebug = "db.pipelinedebug";
    }

    // ============================================================
    // 배치 기록
    // ============================================================

    /// <summary>디버그 엔트리 배치 기록 요청. fire-and-forget (응답은 ack 만 반환).</summary>
    public class AB_Write_Pipeline_Debug_Batch_Request : AB_Message
    {
        /// <summary>저장할 엔트리 리스트. publish 후 호출자는 재사용하지 않는다.</summary>
        public List<AB_Pipeline_Debug_Entry_Model> Entries = new();

        public AB_Write_Pipeline_Debug_Batch_Request() { Topic = AB_Pipeline_Debug_Db_Topics.PipelineDebug; }
    }

    /// <summary>디버그 엔트리 배치 기록 응답.</summary>
    public class AB_Write_Pipeline_Debug_Batch_Response : AB_Message
    {
        public bool Success;
        public int Written;
        public string? Error;
        public AB_Write_Pipeline_Debug_Batch_Response() { Topic = AB_Pipeline_Debug_Db_Topics.PipelineDebug; IsResponse = true; }
    }

    // ============================================================
    // 페이징 조회
    // ============================================================

    /// <summary>세션 기준 페이징 조회 요청.</summary>
    public class AB_Query_Pipeline_Debug_Request : AB_Message
    {
        public string SessionId = "";
        public string? EntryType;
        public int Offset;
        public int Limit = 50;
        public AB_Query_Pipeline_Debug_Request() { Topic = AB_Pipeline_Debug_Db_Topics.PipelineDebug; }
    }

    /// <summary>페이징 조회 응답.</summary>
    public class AB_Query_Pipeline_Debug_Response : AB_Message
    {
        public List<AB_Pipeline_Debug_Entry_Model> Data = new();
        public string? Error;
        public AB_Query_Pipeline_Debug_Response() { Topic = AB_Pipeline_Debug_Db_Topics.PipelineDebug; IsResponse = true; }
    }

    // ============================================================
    // 보존 정리
    // ============================================================

    /// <summary>보존 기한 초과 엔트리 정리 요청.</summary>
    public class AB_Retention_Sweep_Pipeline_Debug_Request : AB_Message
    {
        public int RetentionDays;
        public AB_Retention_Sweep_Pipeline_Debug_Request() { Topic = AB_Pipeline_Debug_Db_Topics.PipelineDebug; }
    }

    /// <summary>보존 정리 응답.</summary>
    public class AB_Retention_Sweep_Pipeline_Debug_Response : AB_Message
    {
        public bool Success;
        public string? Error;
        public AB_Retention_Sweep_Pipeline_Debug_Response() { Topic = AB_Pipeline_Debug_Db_Topics.PipelineDebug; IsResponse = true; }
    }
}
