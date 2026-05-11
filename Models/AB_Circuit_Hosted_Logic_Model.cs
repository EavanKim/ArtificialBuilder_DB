using ArtificialBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>서킷이 호스팅하는 Logic 인스턴스 (Logic UUID 참조 + 그래프 좌표). (example-mental-restructure Phase B Sub 4 Circuit 6/8) — string PK → long PK.</summary>
    [Table("circuit_hosted_logics")]
    public class AB_Circuit_Hosted_Logic_Model
    {
        private long m_id_ = AB_Id_Issuer.Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>호스팅 대상 Logic 의 LogicId (long, Logic DB 참조). 2026-05-11 — string Guid 폐기.</summary>
        private long m_logicUuid_;
        [Required]
        [Column("logic_uuid")]
        public long LogicUuid_
        {
            get { return m_logicUuid_; }
            set { m_logicUuid_ = value; }
        }

        /// <summary>서킷 안 사용 키 (중복 체크).</summary>
        private string m_useKey_ = "";
        [Required]
        [Column("use_key")]
        public string UseKey_
        {
            get { return m_useKey_; }
            set { m_useKey_ = value; }
        }

        private double m_graphX_ = 0;
        [Column("graph_x")]
        public double GraphX_
        {
            get { return m_graphX_; }
            set { m_graphX_ = value; }
        }

        private double m_graphY_ = 0;
        [Column("graph_y")]
        public double GraphY_
        {
            get { return m_graphY_; }
            set { m_graphY_ = value; }
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
