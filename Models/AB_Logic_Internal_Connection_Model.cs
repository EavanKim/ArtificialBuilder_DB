using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>로직 내부 그래프 안의 노드 ↔ 노드 connection. 정본 [[logic-internal-graph]] (v2 2026-05-06 후속).</summary>
    [Table("logic_internal_connections")]
    public class AB_Logic_Internal_Connection_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>출발 노드 (logic_internal_nodes.id, long FK). 2026-05-11 — string → long 정합.</summary>
        private long m_fromNodeId_ = 0;
        [Column("from_node_id")]
        public long FromNodeId_
        {
            get { return m_fromNodeId_; }
            set { m_fromNodeId_ = value; }
        }

        /// <summary>출발 노드의 출력 포트 인덱스.</summary>
        private int m_fromPortIdx_ = 0;
        [Column("from_port_idx")]
        public int FromPortIdx_
        {
            get { return m_fromPortIdx_; }
            set { m_fromPortIdx_ = value; }
        }

        /// <summary>도착 노드 (logic_internal_nodes.id, long FK). 2026-05-11 — string → long 정합.</summary>
        private long m_toNodeId_ = 0;
        [Column("to_node_id")]
        public long ToNodeId_
        {
            get { return m_toNodeId_; }
            set { m_toNodeId_ = value; }
        }

        /// <summary>도착 노드의 입력 포트 인덱스.</summary>
        private int m_toPortIdx_ = 0;
        [Column("to_port_idx")]
        public int ToPortIdx_
        {
            get { return m_toPortIdx_; }
            set { m_toPortIdx_ = value; }
        }

        private DateTime m_createdAt_ = DateTime.UtcNow;
        [Column("created_at")]
        public DateTime CreatedAt_
        {
            get { return m_createdAt_; }
            set { m_createdAt_ = value; }
        }

        public override void Reset()
        {
            base.Reset();
            m_id_ = 0L;
            m_fromNodeId_ = 0;
            m_fromPortIdx_ = 0;
            m_toNodeId_ = 0;
            m_toPortIdx_ = 0;
            m_createdAt_ = default;
        }
    }
}
