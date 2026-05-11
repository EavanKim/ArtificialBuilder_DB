using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>Logic 메타 (단일 row). UUID + 표시 이름 분리. plan: docs/plans/doing/db-three-way-split/1-logic-db-schema.md</summary>
    [Table("logic_meta")]
    public class AB_Logic_Meta_Model
    {
        private long m_id_ = 1;
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private long m_logicUuid_;
        [Required]
        [Column("logic_uuid")]
        public long LogicUuid_
        {
            get { return m_logicUuid_; }
            set { m_logicUuid_ = value; }
        }

        private string m_displayName_ = "";
        [Column("display_name")]
        public string DisplayName_
        {
            get { return m_displayName_; }
            set { m_displayName_ = value; }
        }

        private string? m_description_;
        [Column("description")]
        public string? Description_
        {
            get { return m_description_; }
            set { m_description_ = value; }
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
