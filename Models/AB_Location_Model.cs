using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>장소 정의 (이름/설명).</summary>
    [Table("circuit_locations")]
    public class AB_Location_Model
    {
        private string m_id_ = Guid.NewGuid().ToString();
        [Key]
        [Column("id")]
        public string Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string m_name_ = "";
        [Column("name")]
        public string Name_
        {
            get { return m_name_; }
            set { m_name_ = value; }
        }

        private string? m_description_;
        [Column("description")]
        public string? Description_
        {
            get { return m_description_; }
            set { m_description_ = value; }
        }

        /// <summary>"user" = 사용자 생성, "ai" = AI 생성</summary>
        private string m_source_ = "ai";
        [Column("source")]
        public string Source_
        {
            get { return m_source_; }
            set { m_source_ = value; }
        }

        private int m_sortOrder_ = 0;
        [Column("sort_order")]
        public int SortOrder_
        {
            get { return m_sortOrder_; }
            set { m_sortOrder_ = value; }
        }

        private DateTime m_createdAt_ = DateTime.UtcNow;
        [Column("created_at")]
        public DateTime CreatedAt_
        {
            get { return m_createdAt_; }
            set { m_createdAt_ = value; }
        }

        private DateTime m_updatedAt_ = DateTime.UtcNow;
        [Column("updated_at")]
        public DateTime UpdatedAt_
        {
            get { return m_updatedAt_; }
            set { m_updatedAt_ = value; }
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
