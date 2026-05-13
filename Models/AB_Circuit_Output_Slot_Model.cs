using ArtificialBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>서킷 output 슬롯 (화면 출력 emit). 정본 [[circuit-input-slots]] (v2 2026-05-06 후속).</summary>
    [Table("circuit_output_slots")]
    public class AB_Circuit_Output_Slot_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>사용자 표시 이름.</summary>
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

        /// <summary>매핑된 로직 (Logic 인스턴스 FK, long PK). 2026-05-11 — string Guid → long 마이그.</summary>
        private long m_mappedLogicId_ = 0;
        [Column("mapped_logic_id")]
        public long MappedLogicId_
        {
            get { return m_mappedLogicId_; }
            set { m_mappedLogicId_ = value; }
        }

        /// <summary>매핑된 로직 안의 Output sentinel 노드 (NodeId, long PK). 2026-05-11 — string Guid → long 마이그.</summary>
        private long m_mappedOutputSentinelId_ = 0;
        [Column("mapped_output_sentinel_id")]
        public long MappedOutputSentinelId_
        {
            get { return m_mappedOutputSentinelId_; }
            set { m_mappedOutputSentinelId_ = value; }
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
