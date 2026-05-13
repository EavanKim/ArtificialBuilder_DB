using ArtificialBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>서킷의 hosted logic 별 슬롯 값. 정본 [[circuit-input-slots]] 변수 슬롯 절 (v2 2026-05-06 후속). (example-mental-restructure Phase B Sub 4 Circuit 2/8) — string PK → long PK.</summary>
    [Table("circuit_hosted_logic_slot_values")]
    public class AB_Circuit_Hosted_Logic_Slot_Value_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>대상 hosted logic 의 LogicId — AB_Logic.LogicId (long) 와 매치 정본. 2026-05-11 — string Guid 폐기.</summary>
        private long m_logicId_;
        [Required]
        [Column("logic_id")]
        public long LogicId_
        {
            get { return m_logicId_; }
            set { m_logicId_ = value; }
        }

        /// <summary>대상 슬롯의 Slot_Id_ (= AB_Logic_Variable_Slot_Model.Id_). 2026-05-11 — string FK → long FK 정합.</summary>
        private long m_slotId_ = 0L;
        [Required]
        [Column("slot_id")]
        public long SlotId_
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

        public override void Reset()
        {
            base.Reset();
            m_id_ = 0L;
            m_logicId_ = 0L;
            m_slotId_ = 0L;
            m_valueJson_ = null;
            m_updatedAt_ = default;
        }
    }
}
