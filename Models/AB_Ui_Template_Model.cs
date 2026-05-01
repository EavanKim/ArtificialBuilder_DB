using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 전역 UI 템플릿 (app.db, 모든 Circuit의 시작 라이브러리).
    /// Circuit 인스턴스 단위 활성/정렬 컬럼이 필요한 경우 <see cref="AB_Circuit_Ui_Template_Model"/> 사용.
    /// </summary>
    [Table("ui_templates")]
    public class AB_Ui_Template_Model
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
