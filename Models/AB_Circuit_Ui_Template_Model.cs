using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// TODO[db-three-way-split]: sub 3 폐기 + sub 2 신설. 본 모델 → Response UI DB 의 AB_Response_Ui_Template_Model 로 이전. plan: docs/plans/doing/db-three-way-split/2-response-ui-db-schema.md
namespace ArtificialBuilder.Models
{
    /// <summary>
    /// Circuit 전용 UI 템플릿 (circuit.db, Circuit별 활성/정렬 포함).
    /// 전역 시작 라이브러리는 <see cref="AB_Ui_Template_Model"/> 참조.
    /// </summary>
    [Table("circuit_ui_templates")]
    public class AB_Circuit_Ui_Template_Model
    {
        private string m_id_ = Guid.NewGuid().ToString();
        [Key]
        [Column("id")]
        public string Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string m_name_ = "";
        [Column("name")]
        public string Name_
        {
            get { return m_name_; }
            set { m_name_ = value; }
        }

        private string m_displayMode_ = "chat";
        [Column("display_mode")]
        public string DisplayMode_
        {
            get { return m_displayMode_; }
            set { m_displayMode_ = value; }
        }

        private string? m_xmlContent_;
        [Column("xml_content")]
        public string? XmlContent_
        {
            get { return m_xmlContent_; }
            set { m_xmlContent_ = value; }
        }

        private bool m_isActive_ = false;
        [Column("is_active")]
        public bool IsActive_
        {
            get { return m_isActive_; }
            set { m_isActive_ = value; }
        }

        private int m_sortOrder_ = 0;
        [Column("sort_order")]
        public int SortOrder_
        {
            get { return m_sortOrder_; }
            set { m_sortOrder_ = value; }
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
