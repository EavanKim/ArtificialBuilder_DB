using ArtificialBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>서킷 안 노드(로직) 가 invoke 한 다른 서킷 list (호출 노드 id / 대상 서킷 이름 / 사용 키). (example-mental-restructure Phase B Sub 4 Circuit 1/8) — string PK → long PK.</summary>
    [Table("circuit_used_sub_circuits")]
    public class AB_Circuit_Used_Sub_Circuit_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>호출하는 노드(로직) id (그래프 안, long). 2026-05-11 — string → long 정합.</summary>
        private long m_callerNodeId_ = 0;
        [Column("caller_node_id")]
        public long CallerNodeId_
        {
            get { return m_callerNodeId_; }
            set { m_callerNodeId_ = value; }
        }

        /// <summary>대상 서킷 이름.</summary>
        private string m_targetCircuitName_ = "";
        [Required]
        [Column("target_circuit_name")]
        public string TargetCircuitName_
        {
            get { return m_targetCircuitName_; }
            set { m_targetCircuitName_ = value; }
        }

        /// <summary>사용 키 (중복 체크).</summary>
        private string m_useKey_ = "";
        [Required]
        [Column("use_key")]
        public string UseKey_
        {
            get { return m_useKey_; }
            set { m_useKey_ = value; }
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
            m_callerNodeId_ = 0;
            m_targetCircuitName_ = "";
            m_useKey_ = "";
            m_createdAt_ = default;
            m_updatedAt_ = default;
        }
    }
}
