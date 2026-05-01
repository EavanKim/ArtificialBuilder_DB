using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>두 캐릭터 간 관계 정의 (유형/설명).</summary>
    [Table("circuit_character_relationships")]
    public class AB_Character_Relationship_Model
    {
        private string m_id_ = Guid.NewGuid().ToString();
        [Key]
        [Column("id")]
        public string Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>관계 주체 캐릭터 ID</summary>
        private string m_characterId_ = "";
        [Column("character_id")]
        public string CharacterId_
        {
            get { return m_characterId_; }
            set { m_characterId_ = value; }
        }

        /// <summary>관계 대상 캐릭터 ID</summary>
        private string m_targetId_ = "";
        [Column("target_id")]
        public string TargetId_
        {
            get { return m_targetId_; }
            set { m_targetId_ = value; }
        }

        /// <summary>관계 유형 (예: friend, rival, family, lover, enemy, ally, mentor, student)</summary>
        private string m_relationType_ = "";
        [Column("relation_type")]
        public string RelationType_
        {
            get { return m_relationType_; }
            set { m_relationType_ = value; }
        }

        /// <summary>관계 상세 설명</summary>
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
