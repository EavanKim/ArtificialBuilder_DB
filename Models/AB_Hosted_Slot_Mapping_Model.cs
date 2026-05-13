using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 서킷 hosted_logic 의 슬롯 매핑 1 row — 로직 schema 의 빈 슬롯명 → 칠판 데이터 키 + description.
    /// 정본: docs/architecture/3-pipeline/circuit-as-node-graph.md, docs/architecture/0-principles/data-by-blackboard-key-only.md
    /// 데이터 자체 X — 키 + description (Kind / TypeCode / Note) 만 보유 ([[data-by-blackboard-key-only]]).
    /// </summary>
    [Table("circuit_hosted_logic_slot_mappings")]
    public class AB_Hosted_Slot_Mapping_Model
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_ { get { return m_id_; } set { m_id_ = value; } }

        /// <summary>FK → AB_Circuit_Hosted_Logic_Model.Id_ (long PK). 2026-05-11 — string FK → long FK 정합.</summary>
        private long m_hostedLogicId_ = 0L;
        [Required]
        [Column("hosted_logic_id")]
        public long HostedLogicId_ { get { return m_hostedLogicId_; } set { m_hostedLogicId_ = value; } }

        /// <summary>로직 schema 의 빈 슬롯명 (예: "vector_store" / "asset_image" / "var_user_name").</summary>
        private string m_slotName_ = "";
        [Required]
        [Column("slot_name")]
        public string SlotName_ { get { return m_slotName_; } set { m_slotName_ = value; } }

        /// <summary>매핑할 칠판 데이터 키.</summary>
        private string m_dataKey_ = "";
        [Required]
        [Column("data_key")]
        public string DataKey_ { get { return m_dataKey_; } set { m_dataKey_ = value; } }

        /// <summary>
        /// description — 데이터 종류 식별자 (open key 모델, [[open-key-io]]).
        /// **자유 string** — enum 강제 X. 받는 쪽 (인터프리터) 이 description 보고 처리 가능 여부 자체 판단:
        /// 처리 가능 = 정상 처리, 처리 불가능 = 예외 X / ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn 로깅.
        /// </summary>
        private string m_descKind_ = "";
        [Column("desc_kind")]
        public string DescKind_ { get { return m_descKind_; } set { m_descKind_ = value; } }

        /// <summary>description — 숫자 type code (sub-kind 또는 format hint, 0 = 미지정).</summary>
        private int m_descTypeCode_;
        [Column("desc_type_code")]
        public int DescTypeCode_ { get { return m_descTypeCode_; } set { m_descTypeCode_ = value; } }

        /// <summary>description — 자유 노트.</summary>
        private string? m_descNote_;
        [Column("desc_note")]
        public string? DescNote_ { get { return m_descNote_; } set { m_descNote_ = value; } }

        private DateTime m_createdAt_ = DateTime.UtcNow;
        [Column("created_at")]
        public DateTime CreatedAt_ { get { return m_createdAt_; } set { m_createdAt_ = value; } }

        private DateTime m_updatedAt_ = DateTime.UtcNow;
        [Column("updated_at")]
        public DateTime UpdatedAt_ { get { return m_updatedAt_; } set { m_updatedAt_ = value; } }
    }
}
