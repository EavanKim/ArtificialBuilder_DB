using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// TODO[db-three-way-split]: sub 3 폐기 + sub 2 신설. 본 모델 → Response UI DB 의 AB_Response_Ui_Window_Model 로 이전. UUID + 표시 이름 분리. plan: docs/plans/doing/db-three-way-split/2-response-ui-db-schema.md
namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 응답 윈도우 설정. Circuit DB에 저장되어 Circuit과 함께 배포됨.
    /// 노드 에디터의 End 노드 + Response UI 바인딩이 WindowId로 참조.
    /// 성격/프레임/레이아웃/뎁스는 window_components (AB_Window_Component_Model) 에 저장.
    /// </summary>
    [Table("response_windows")]
    public class AB_Response_Window_Model
    {
        private string m_id_ = Guid.NewGuid().ToString();
        [Key]
        [Column("id")]
        public string Id_
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
