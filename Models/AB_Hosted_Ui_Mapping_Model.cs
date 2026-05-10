using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 서킷 hosted_logic 의 UI 출력 매핑 1 row — 로직 안의 어느 내부 노드 출력 → 어느 Response UI 슬롯 + description.
    /// 정본: docs/architecture/3-pipeline/circuit-as-node-graph.md, docs/architecture/0-principles/data-by-blackboard-key-only.md
    /// 사용자 예시 (2026-05-07): 메시지 창 = VLM+LLM 결과 / 이미지 창 = VLM 결과만.
    /// 데이터 자체 X — 노드 id + UI 슬롯명 + description ([[data-by-blackboard-key-only]]).
    /// </summary>
    [Table("circuit_hosted_logic_ui_mappings")]
    public class AB_Hosted_Ui_Mapping_Model
    {
        private long m_id_ = ArtificialBuilder.AB_Id_Issuer.Issue();
        [Key]
        [Column("id")]
        public long Id_ { get { return m_id_; } set { m_id_ = value; } }

        /// <summary>FK → AB_Circuit_Hosted_Logic_Model.Id_</summary>
        private string m_hostedLogicId_ = "";
        [Required]
        [Column("hosted_logic_id")]
        public string HostedLogicId_ { get { return m_hostedLogicId_; } set { m_hostedLogicId_ = value; } }

        /// <summary>로직 안의 내부 노드 id (Logic DB 의 logic_internal_nodes.id 참조).</summary>
        private string m_internalNodeId_ = "";
        [Required]
        [Column("internal_node_id")]
        public string InternalNodeId_ { get { return m_internalNodeId_; } set { m_internalNodeId_ = value; } }

        /// <summary>대상 Response UI 슬롯명 (예: "message" / "image" / "model_3d").</summary>
        private string m_uiSlotName_ = "";
        [Required]
        [Column("ui_slot_name")]
        public string UiSlotName_ { get { return m_uiSlotName_; } set { m_uiSlotName_ = value; } }

        /// <summary>
        /// description — 데이터 종류 식별자 (open key 모델, [[open-key-io]]).
        /// **자유 string** — enum 강제 X. UI 슬롯 (받는 쪽) 이 description 보고 처리 가능 여부 자체 판단:
        /// 처리 가능 = 표시, 처리 불가능 = 예외 X / AB_Log.Warn 로깅.
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
