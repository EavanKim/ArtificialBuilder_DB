using ArtificialBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>로어북 엔트리 (키워드 매칭으로 컨텍스트에 주입). (example-mental-restructure Phase B Sub 4 Circuit 5/8) — string PK → long PK.</summary>
    [Table("lore_entries")]
    public class AB_Lore_Entry_Model : ArtificialBuilder_EDP.Core.AB_Object
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

        private string m_keywords_ = "";
        [Column("keywords")]
        public string Keywords_
        {
            get { return m_keywords_; }
            set { m_keywords_ = value; }
        }

        private string m_content_ = "";
        [Column("content")]
        public string Content_
        {
            get { return m_content_; }
            set { m_content_ = value; }
        }

        private bool m_enabled_ = true;
        [Column("enabled")]
        public bool Enabled_
        {
            get { return m_enabled_; }
            set { m_enabled_ = value; }
        }

        private int m_priority_ = 0;
        [Column("priority")]
        public int Priority_
        {
            get { return m_priority_; }
            set { m_priority_ = value; }
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
            m_keywords_ = "";
            m_content_ = "";
            m_enabled_ = true;
            m_priority_ = 0;
            m_createdAt_ = default;
            m_updatedAt_ = default;
        }
    }
}
