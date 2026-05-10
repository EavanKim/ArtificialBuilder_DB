using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>로직 변수 슬롯 (빈칸 schema). 정본 [[circuit-input-slots]] 변수 슬롯 절 (v2 2026-05-06 후속).</summary>
    [Table("logic_variable_slots")]
    public class AB_Logic_Variable_Slot_Model
    {
        private long m_id_ = ArtificialBuilder.AB_Id_Issuer.Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>슬롯 이름 (사용자 표시 + 노드 ref 키).</summary>
        private string m_name_ = "";
        [Required]
        [Column("name")]
        public string Name_
        {
            get { return m_name_; }
            set { m_name_ = value; }
        }

        /// <summary>전송 데이터 타입.</summary>
        private string m_valueKind_ = "Text";
        [Required]
        [Column("value_kind")]
        public string ValueKind_
        {
            get { return m_valueKind_; }
            set { m_valueKind_ = value; }
        }

        /// <summary>슬롯 의도 분류 (Model / Prompt / Number / Bool / Text / ...).</summary>
        private string m_slotIntent_ = "Text";
        [Required]
        [Column("slot_intent")]
        public string SlotIntent_
        {
            get { return m_slotIntent_; }
            set { m_slotIntent_ = value; }
        }

        /// <summary>옵션: 기본값 JSON.</summary>
        private string? m_defaultValueJson_;
        [Column("default_value_json")]
        public string? DefaultValueJson_
        {
            get { return m_defaultValueJson_; }
            set { m_defaultValueJson_ = value; }
        }

        /// <summary>옵션: 사용자 도움말 (UI tooltip).</summary>
        private string? m_description_;
        [Column("description")]
        public string? Description_
        {
            get { return m_description_; }
            set { m_description_ = value; }
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
