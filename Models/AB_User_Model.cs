using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDPFW.Models
{
    /// <summary>EDPFW 표준 사용자 모델.</summary>
    [Table("users")]
    public class AB_User_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_;
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string? m_username_;
        [Column("username")]
        public string? Username_
        {
            get { return m_username_; }
            set { m_username_ = value; }
        }

        private string? m_email_;
        [Column("email")]
        public string? Email_
        {
            get { return m_email_; }
            set { m_email_ = value; }
        }

        private DateTime m_createdAt_;
        [Column("created_at")]
        public DateTime CreatedAt_
        {
            get { return m_createdAt_; }
            set { m_createdAt_ = value; }
        }

        public override void Reset()
        {
            base.Reset();
            m_id_ = 0L;
            m_username_ = null;
            m_email_ = null;
            m_createdAt_ = default;
        }
    }
}
