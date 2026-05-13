using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>Response UI 메타 (단일 row). UUID + 표시 이름 분리. plan: docs/plans/doing/db-three-way-split/2-response-ui-db-schema.md</summary>
    [Table("response_ui_meta")]
    public class AB_Response_Ui_Meta_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = 1;
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string m_responseUiUuid_ = Guid.NewGuid().ToString();
        [Required]
        [Column("response_ui_uuid")]
        public string ResponseUiUuid_
        {
            get { return m_responseUiUuid_; }
            set { m_responseUiUuid_ = value; }
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

        public override void Reset()
        {
            base.Reset();
            m_id_ = 1;
            m_responseUiUuid_ = "";
            m_displayName_ = "";
            m_description_ = null;
            m_createdAt_ = default;
            m_updatedAt_ = default;
        }
    }
}
