using ArtificialBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>장소 간 이동 경로 (양방향). A↔B 연결을 나타냄. (example-mental-restructure Phase B Sub 4 Persona 4/7) — string PK → long PK.</summary>
    [Table("circuit_location_connections")]
    public class AB_Location_Connection_Model
    {
        private long m_id_ = AB_Id_Issuer.Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string m_fromLocationId_ = "";
        [Column("from_location_id")]
        public string FromLocationId_
        {
            get { return m_fromLocationId_; }
            set { m_fromLocationId_ = value; }
        }

        private string m_toLocationId_ = "";
        [Column("to_location_id")]
        public string ToLocationId_
        {
            get { return m_toLocationId_; }
            set { m_toLocationId_ = value; }
        }

        /// <summary>경로 설명 (예: "숲길", "지하 통로")</summary>
        private string? m_pathLabel_;
        [Column("path_label")]
        public string? PathLabel_
        {
            get { return m_pathLabel_; }
            set { m_pathLabel_ = value; }
        }

        /// <summary>"user" = 사용자 생성, "ai" = AI 생성</summary>
        private string m_source_ = "ai";
        [Column("source")]
        public string Source_
        {
            get { return m_source_; }
            set { m_source_ = value; }
        }

        private DateTime m_createdAt_ = DateTime.UtcNow;
        [Column("created_at")]
        public DateTime CreatedAt_
        {
            get { return m_createdAt_; }
            set { m_createdAt_ = value; }
        }

        /// <summary>null=Circuit 템플릿, 값=채팅전용</summary>
        private string? m_sessionId_;
        [Column("session_id")]
        public string? SessionId_
        {
            get { return m_sessionId_; }
            set { m_sessionId_ = value; }
        }
    }
}
