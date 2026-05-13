using ArtificialBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>캐릭터 정의 (이름/성격/인사말/배경). Circuit 템플릿 또는 세션 전용. (example-mental-restructure Phase B Sub 4 Persona 6/7) — string PK → long PK.</summary>
    [Table("circuit_characters")]
    public class AB_Character_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
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

        private string? m_personality_;
        [Column("personality")]
        public string? Personality_
        {
            get { return m_personality_; }
            set { m_personality_ = value; }
        }

        private string? m_greeting_;
        [Column("greeting")]
        public string? Greeting_
        {
            get { return m_greeting_; }
            set { m_greeting_ = value; }
        }

        private string? m_backstory_;
        [Column("backstory")]
        public string? Backstory_
        {
            get { return m_backstory_; }
            set { m_backstory_ = value; }
        }

        private string? m_creatorNotes_;
        [Column("creator_notes")]
        public string? CreatorNotes_
        {
            get { return m_creatorNotes_; }
            set { m_creatorNotes_ = value; }
        }

        /// <summary>"user" = 사용자 생성 (풀 스펙), "ai" = AI 생성 NPC (기본 정보만)</summary>
        private string m_source_ = "user";
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
        private long? m_sessionId_;
        [Column("session_id")]
        public long? SessionId_
        {
            get { return m_sessionId_; }
            set { m_sessionId_ = value; }
        }

        public override void Reset()
        {
            base.Reset();
            m_id_ = 0L;
            m_name_ = "";
            m_personality_ = null;
            m_greeting_ = null;
            m_backstory_ = null;
            m_creatorNotes_ = null;
            m_source_ = "user";
            m_sortOrder_ = 0;
            m_createdAt_ = default;
            m_updatedAt_ = default;
            m_sessionId_ = null;
        }
    }
}
