using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>Response UI 의 Layer 정의 ([[layer-targeting]] — UUID + 사용자 표시 이름 분리). plan: docs/plans/doing/db-three-way-split/2-response-ui-db-schema.md</summary>
    [Table("response_ui_layers")]
    public class AB_Response_Ui_Layer_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>소속 Window (response_ui_windows.id) 참조.</summary>
        private string m_windowId_ = "";
        [Required]
        [Column("window_id")]
        public string WindowId_
        {
            get { return m_windowId_; }
            set { m_windowId_ = value; }
        }

        /// <summary>Layer 표시 이름 (사용자 변경 가능).</summary>
        private string m_displayName_ = "";
        [Column("display_name")]
        public string DisplayName_
        {
            get { return m_displayName_; }
            set { m_displayName_ = value; }
        }

        /// <summary>Layer 종류 태그 (message / image / 3d 등 출력 레이어).</summary>
        private string m_layerType_ = "";
        [Required]
        [Column("layer_type")]
        public string LayerType_
        {
            get { return m_layerType_; }
            set { m_layerType_ = value; }
        }

        private int m_sortOrder_ = 0;
        [Column("sort_order")]
        public int SortOrder_
        {
            get { return m_sortOrder_; }
            set { m_sortOrder_ = value; }
        }

        private bool m_visible_ = true;
        [Column("visible")]
        public bool Visible_
        {
            get { return m_visible_; }
            set { m_visible_ = value; }
        }

        private string? m_configJson_;
        [Column("config_json")]
        public string? ConfigJson_
        {
            get { return m_configJson_; }
            set { m_configJson_ = value; }
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
