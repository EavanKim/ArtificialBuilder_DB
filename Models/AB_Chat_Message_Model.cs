using System;
using ArtificialBuilder;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 채팅 메시지 DTO. chat_messages 테이블은 폐기됐고, 이 타입은 순수 POCO 로
    /// `AB_Chat.GetMessagesAsync` view 가 context_records 에서 합성해 반환하는 용도.
    /// </summary>
    public class AB_Chat_Message_Model
    {
        private long m_id_;
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string m_sessionId_ = "";
        public string SessionId_
        {
            get { return m_sessionId_; }
            set { m_sessionId_ = value; }
        }

        private string m_role_ = "";
        public string Role_
        {
            get { return m_role_; }
            set { m_role_ = value; }
        }

        private string m_content_ = "";
        public string Content_
        {
            get { return m_content_; }
            set { m_content_ = value; }
        }

        private string? m_modelId_;
        public string? ModelId_
        {
            get { return m_modelId_; }
            set { m_modelId_ = value; }
        }

        private int m_inputTokens_;
        public int InputTokens_
        {
            get { return m_inputTokens_; }
            set { m_inputTokens_ = value; }
        }

        private int m_outputTokens_;
        public int OutputTokens_
        {
            get { return m_outputTokens_; }
            set { m_outputTokens_ = value; }
        }

        private string? m_requestBody_;
        public string? RequestBody_
        {
            get { return m_requestBody_; }
            set { m_requestBody_ = value; }
        }

        private decimal m_cost_;
        public decimal Cost_
        {
            get { return m_cost_; }
            set { m_cost_ = value; }
        }

        private string? m_pipelineTrace_;
        public string? PipelineTrace_
        {
            get { return m_pipelineTrace_; }
            set { m_pipelineTrace_ = value; }
        }

        private long? m_parentId_;
        public long? ParentId_
        {
            get { return m_parentId_; }
            set { m_parentId_ = value; }
        }

        private bool m_isActive_ = true;
        public bool IsActive_
        {
            get { return m_isActive_; }
            set { m_isActive_ = value; }
        }

        private DateTime m_createdAt_ = DateTime.UtcNow;
        public DateTime CreatedAt_
        {
            get { return m_createdAt_; }
            set { m_createdAt_ = value; }
        }

        private long m_outputAt_;
        public long OutputAt_
        {
            get { return m_outputAt_; }
            set { m_outputAt_ = value; }
        }

        private string? m_imageData_;
        public string? ImageData_
        {
            get { return m_imageData_; }
            set { m_imageData_ = value; }
        }

        // context_record 키 — 내부 Data 아이템에 Context_Record_Key 로 전달돼 ↻/✎/✕ 명령 대상으로 사용.

        private string m_nodeId_ = "";
        public string NodeId_
        {
            get { return m_nodeId_; }
            set { m_nodeId_ = value; }
        }

        private int m_turnIndex_;
        public int TurnIndex_
        {
            get { return m_turnIndex_; }
            set { m_turnIndex_ = value; }
        }

        private int m_refreshIndex_;
        public int RefreshIndex_
        {
            get { return m_refreshIndex_; }
            set { m_refreshIndex_ = value; }
        }

        private int m_emissionOrder_;
        public int EmissionOrder_
        {
            get { return m_emissionOrder_; }
            set { m_emissionOrder_ = value; }
        }

        /// <summary>이 view 메시지의 context_record 식별 키.</summary>
        public Context_Record_Key ToContextRecordKey()
        {
            return new Context_Record_Key(m_sessionId_, m_nodeId_, m_turnIndex_, m_refreshIndex_, m_emissionOrder_);
        }
    }
}
