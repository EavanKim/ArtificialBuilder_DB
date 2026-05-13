using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>Logic 안 sub-logic 참조 (다른 Logic 의 UUID — "로직 내 로직" 재귀 정합). plan: docs/plans/doing/db-three-way-split/1-logic-db-schema.md</summary>
    [Table("logic_sub_logics")]
    public class AB_Logic_Sub_Logic_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private long m_subLogicUuid_;
        [Required]
        [Column("sub_logic_uuid")]
        public long SubLogicUuid_
        {
            get { return m_subLogicUuid_; }
            set { m_subLogicUuid_ = value; }
        }

        private string m_useKey_ = "";
        [Required]
        [Column("use_key")]
        public string UseKey_
        {
            get { return m_useKey_; }
            set { m_useKey_ = value; }
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
