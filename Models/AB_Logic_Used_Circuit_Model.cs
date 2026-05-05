using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>Logic 이 사용하는 Circuit 항목 (이름 + 사용 키 + 서킷 내 system prompt + 끌어쓴 model key). plan: docs/plans/doing/db-three-way-split/1-logic-db-schema.md</summary>
    [Table("logic_used_circuits")]
    public class AB_Logic_Used_Circuit_Model
    {
        private string m_id_ = Guid.NewGuid().ToString();
        [Key]
        [Column("id")]
        public string Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string m_circuitName_ = "";
        [Required]
        [Column("circuit_name")]
        public string CircuitName_
        {
            get { return m_circuitName_; }
            set { m_circuitName_ = value; }
        }

        private string m_useKey_ = "";
        [Required]
        [Column("use_key")]
        public string UseKey_
        {
            get { return m_useKey_; }
            set { m_useKey_ = value; }
        }

        private string? m_systemPrompt_;
        [Column("system_prompt")]
        public string? SystemPrompt_
        {
            get { return m_systemPrompt_; }
            set { m_systemPrompt_ = value; }
        }

        private string? m_modelConfigId_;
        [Column("model_config_id")]
        public string? ModelConfigId_
        {
            get { return m_modelConfigId_; }
            set { m_modelConfigId_ = value; }
        }

        private int m_orderIndex_ = 0;
        [Column("order_index")]
        public int OrderIndex_
        {
            get { return m_orderIndex_; }
            set { m_orderIndex_ = value; }
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
