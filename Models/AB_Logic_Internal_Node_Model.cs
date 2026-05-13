using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>로직 내부 그래프 안의 노드 (= AB_Logic_Node_* 인스턴스). 정본 [[logic-internal-graph]] (v2 2026-05-06 후속).</summary>
    [Table("logic_internal_nodes")]
    public class AB_Logic_Internal_Node_Model
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>노드 종류 (Input / Output / AI_LLM / AI_VLM / AI_ALM / AI_VGM / Llm / Switch / Const / Regex / ...).</summary>
        private string m_kind_ = "";
        [Required]
        [Column("kind")]
        public string Kind_
        {
            get { return m_kind_; }
            set { m_kind_ = value; }
        }

        /// <summary>로직 에디터 캔버스 X.</summary>
        private double m_canvasX_ = 0;
        [Column("canvas_x")]
        public double CanvasX_
        {
            get { return m_canvasX_; }
            set { m_canvasX_ = value; }
        }

        /// <summary>로직 에디터 캔버스 Y.</summary>
        private double m_canvasY_ = 0;
        [Column("canvas_y")]
        public double CanvasY_
        {
            get { return m_canvasY_; }
            set { m_canvasY_ = value; }
        }

        /// <summary>노드별 속성 JSON (모델 ID / SystemPrompt / RequestBody / 토큰 한계 등).</summary>
        private string? m_propertiesJson_;
        [Column("properties_json")]
        public string? PropertiesJson_
        {
            get { return m_propertiesJson_; }
            set { m_propertiesJson_ = value; }
        }

        /// <summary>대응되는 외부 connector idx (Input/Output sentinel 만 사용. -1 = 본체 노드).</summary>
        private int m_externalConnectorIndex_ = -1;
        [Column("external_connector_index")]
        public int ExternalConnectorIndex_
        {
            get { return m_externalConnectorIndex_; }
            set { m_externalConnectorIndex_ = value; }
        }

        /// <summary>
        /// 추상 input port schema list (JSON, List&lt;AB_Node_Port_Schema&gt; 직렬화).
        /// 모델 ≠ 노드 — 노드는 *추상 인터페이스*만 ([[open-key-io]]). 빈 list = port 없음.
        /// </summary>
        private string? m_inputsJson_ = "[]";
        [Column("inputs_json")]
        public string? InputsJson_
        {
            get { return m_inputsJson_; }
            set { m_inputsJson_ = value; }
        }

        /// <summary>추상 output port schema list (JSON, List&lt;AB_Node_Port_Schema&gt; 직렬화).</summary>
        private string? m_outputsJson_ = "[]";
        [Column("outputs_json")]
        public string? OutputsJson_
        {
            get { return m_outputsJson_; }
            set { m_outputsJson_ = value; }
        }

        /// <summary>
        /// 별도 벡터 저장소 사용자 토글 — 사용 가능 콘크리트 (AB_Logic_Node.HasVectorStorage_ override true, 예: LLM) 안에서만 의미.
        /// 사용 불가능 콘크리트는 false 고정. Phase C 에서 인터프리터가 콘크리트 virtual + 본 toggle 검사 후 Factory.Create.
        /// 정본: docs/plans/todo/node-vector-storage-factory/README.md.
        /// </summary>
        private bool m_vectorStorageEnabled_;
        [Column("vector_storage_enabled")]
        public bool VectorStorageEnabled_
        {
            get { return m_vectorStorageEnabled_; }
            set { m_vectorStorageEnabled_ = value; }
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
