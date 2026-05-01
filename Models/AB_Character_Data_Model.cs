using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>캐릭터별 동적 데이터 항목 (필드명/값 + 카테고리). 세션 또는 Circuit 범위.</summary>
    [Table("circuit_character_data")]
    public class AB_Character_Data_Model
    {
        private string m_id_ = Guid.NewGuid().ToString();
        [Key]
        [Column("id")]
        public string Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string m_characterId_ = "";
        [Column("character_id")]
        public string CharacterId_
        {
            get { return m_characterId_; }
            set { m_characterId_ = value; }
        }

        private string? m_sessionId_;
        [Column("session_id")]
        public string? SessionId_
        {
            get { return m_sessionId_; }
            set { m_sessionId_ = value; }
        }

        private string m_categoryId_ = "";
        [Column("category_id")]
        public string CategoryId_
        {
            get { return m_categoryId_; }
            set { m_categoryId_ = value; }
        }

        private string m_fieldName_ = "";
        [Column("field_name")]
        public string FieldName_
        {
            get { return m_fieldName_; }
            set { m_fieldName_ = value; }
        }

        private string? m_fieldValue_;
        [Column("field_value")]
        public string? FieldValue_
        {
            get { return m_fieldValue_; }
            set { m_fieldValue_ = value; }
        }

        private string? m_narrative_;
        [Column("narrative")]
        public string? Narrative_
        {
            get { return m_narrative_; }
            set { m_narrative_ = value; }
        }

        /// <summary>"user", "ai", "pattern"</summary>
        private string m_source_ = "user";
        [Column("source")]
        public string Source_
        {
            get { return m_source_; }
            set { m_source_ = value; }
        }

        /// <summary>연결된 채팅 메시지 ID (cascade 삭제용). null=수동입력</summary>
        private long? m_messageId_;
        [Column("message_id")]
        public long? MessageId_
        {
            get { return m_messageId_; }
            set { m_messageId_ = value; }
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
    }
}
