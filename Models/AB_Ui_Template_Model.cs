using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 전역 UI 템플릿 (app.db, 모든 Circuit의 시작 라이브러리).
    /// Circuit 인스턴스 단위 활성/정렬 컬럼이 필요한 경우 <see cref="AB_Response_Ui_Template_Model"/> 사용.
    /// (example-mental-restructure Phase B Sub 4 Response_Ui Ui_Template) — string PK → long PK.
    /// </summary>
    [Table("ui_templates")]
    public class AB_Ui_Template_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
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

        public override void Reset()
        {
            base.Reset();
            m_id_ = 0L;
            m_name_ = "";
            m_displayMode_ = "chat";
            m_xmlContent_ = null;
            m_createdAt_ = default;
            m_updatedAt_ = default;
        }
    }
}
