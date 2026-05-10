using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArtificialBuilder;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// Response UI 의 Window 정의 (Response UI DB per-response-ui SQLite). 로직 에디터의 End 노드 + Response UI 바인딩이 WindowId 로 참조.
    /// 성격/프레임/레이아웃/뎁스는 response_ui_components (AB_Response_Ui_Component_Model) 에 저장.
    /// (example-mental-restructure-phase-b-residue sub 1) — string PK → long PK.
    /// </summary>
    [Table("response_ui_windows")]
    public class AB_Response_Ui_Window_Model
    {
        private long m_id_ = AB_Id_Issuer.Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>윈도우 표시 이름</summary>
        private string m_name_ = "";
        [Required]
        [Column("name")]
        public string Name_
        {
            get { return m_name_; }
            set { m_name_ = value; }
        }

        /// <summary>화면 배치: top, center, bottom, overlay</summary>
        private string m_position_ = "center";
        [Column("position")]
        public string Position_
        {
            get { return m_position_; }
            set { m_position_ = value; }
        }

        /// <summary>활성화 여부</summary>
        private bool m_enabled_ = true;
        [Column("enabled")]
        public bool Enabled_
        {
            get { return m_enabled_; }
            set { m_enabled_ = value; }
        }

        private double m_width_ = 0;
        [Column("width")]
        public double Width_
        {
            get { return m_width_; }
            set { m_width_ = value; }
        }

        private double m_height_ = 0;
        [Column("height")]
        public double Height_
        {
            get { return m_height_; }
            set { m_height_ = value; }
        }

        private string? m_xmlTemplate_;
        [Column("xml_template")]
        public string? XmlTemplate_
        {
            get { return m_xmlTemplate_; }
            set { m_xmlTemplate_ = value; }
        }

        private string? m_background_;
        [Column("background")]
        public string? Background_
        {
            get { return m_background_; }
            set { m_background_ = value; }
        }

        private int m_sortOrder_ = 0;
        [Column("sort_order")]
        public int SortOrder_
        {
            get { return m_sortOrder_; }
            set { m_sortOrder_ = value; }
        }

        private string? m_styleJson_;
        [Column("style_json")]
        public string? StyleJson_
        {
            get { return m_styleJson_; }
            set { m_styleJson_ = value; }
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
