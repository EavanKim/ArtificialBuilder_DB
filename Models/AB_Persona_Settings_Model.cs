using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>페르소나 단일 설정 (사용자 프롬프트/아이콘). singleton (PK fixed = 1). (example-mental-restructure Phase B Sub 4 Persona 3/7) — string PK → long PK.</summary>
    [Table("persona_settings")]
    public class AB_Persona_Settings_Model
    {
        private long m_id_ = 1;
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string? m_prompt_;
        [Column("prompt")]
        public string? Prompt_
        {
            get { return m_prompt_; }
            set { m_prompt_ = value; }
        }

        private byte[]? m_iconData_;
        [Column("icon_data")]
        public byte[]? IconData_
        {
            get { return m_iconData_; }
            set { m_iconData_ = value; }
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
