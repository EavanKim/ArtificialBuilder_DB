using ArtificialBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>서킷 input 슬롯 (사용자 키보드 입력 진입). 정본 [[circuit-input-slots]] (v2 2026-05-06 후속).</summary>
    [Table("circuit_input_slots")]
    public class AB_Circuit_Input_Slot_Model : ArtificialBuilder_EDP.Core.AB_Object
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

        /// <summary>전송 데이터 타입 (None / ChatData / Text).</summary>
        private string m_valueKind_ = "Text";
        [Required]
        [Column("value_kind")]
        public string ValueKind_
        {
            get { return m_valueKind_; }
            set { m_valueKind_ = value; }
        }

        /// <summary>기본값 JSON (옵션).</summary>
        private string? m_defaultValueJson_;
        [Column("default_value_json")]
        public string? DefaultValueJson_
        {
            get { return m_defaultValueJson_; }
            set { m_defaultValueJson_ = value; }
        }

        /// <summary>매핑된 로직 (Logic 인스턴스 FK, long PK). 2026-05-11 — string Guid → long 마이그.</summary>
        private long m_mappedLogicId_ = 0;
        [Column("mapped_logic_id")]
        public long MappedLogicId_
        {
            get { return m_mappedLogicId_; }
            set { m_mappedLogicId_ = value; }
        }

        /// <summary>매핑된 로직 안의 Input sentinel 노드 (NodeId, long PK). 2026-05-11 — string Guid → long 마이그.</summary>
        private long m_mappedInputSentinelId_ = 0;
        [Column("mapped_input_sentinel_id")]
        public long MappedInputSentinelId_
        {
            get { return m_mappedInputSentinelId_; }
            set { m_mappedInputSentinelId_ = value; }
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

        public override void Reset()
        {
            base.Reset();
            m_id_ = 0L;
            m_name_ = "";
            m_valueKind_ = "Text";
            m_defaultValueJson_ = null;
            m_mappedLogicId_ = 0;
            m_mappedInputSentinelId_ = 0;
            m_createdAt_ = default;
            m_updatedAt_ = default;
        }
    }
}
