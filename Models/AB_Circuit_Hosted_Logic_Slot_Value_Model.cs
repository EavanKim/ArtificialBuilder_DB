using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>서킷의 hosted logic 별 슬롯 값. 정본 [[circuit-input-slots]] 변수 슬롯 절 (v2 2026-05-06 후속).</summary>
    [Table("circuit_hosted_logic_slot_values")]
    public class AB_Circuit_Hosted_Logic_Slot_Value_Model
    {
        private string m_id_ = Guid.NewGuid().ToString();
        [Key]
        [Column("id")]
        public string Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>대상 hosted logic 의 LogicId.</summary>
        private string m_logicId_ = "";
        [Required]
        [Column("logic_id")]
        public string LogicId_
        {
            get { return m_logicId_; }
            set { m_logicId_ = value; }
        }

        /// <summary>대상 슬롯의 Slot_Id_ (= AB_Logic_Variable_Slot_Model.Id_).</summary>
        private string m_slotId_ = "";
        [Required]
        [Column("slot_id")]
        public string SlotId_
        {
            get { return m_slotId_; }
            set { m_slotId_ = value; }
        }

        /// <summary>슬롯 값 — JSON 인코딩.</summary>
        private string? m_valueJson_;
        [Column("value_json")]
        public string? ValueJson_
        {
            get { return m_valueJson_; }
            set { m_valueJson_ = value; }
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
