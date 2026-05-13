using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>응답 본문에서 캐릭터/관계/장소를 추출하는 패턴 설정.</summary>
    [Table("circuit_pattern_configs")]
    public class AB_Pattern_Config_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>"character", "relationship", "location", "connection"</summary>
        private string m_patternType_ = "";
        [Column("pattern_type")]
        public string PatternType_
        {
            get { return m_patternType_; }
            set { m_patternType_ = value; }
        }

        private bool m_enabled_ = false;
        [Column("enabled")]
        public bool Enabled_
        {
            get { return m_enabled_; }
            set { m_enabled_ = value; }
        }

        /// <summary>"regex", "delimiter", "free"</summary>
        private string m_mode_ = "regex";
        [Column("mode")]
        public string Mode_
        {
            get { return m_mode_; }
            set { m_mode_ = value; }
        }

        /// <summary>regex 패턴 또는 구분자 모드 JSON</summary>
        private string m_patternText_ = "";
        [Column("pattern_text")]
        public string PatternText_
        {
            get { return m_patternText_; }
            set { m_patternText_ = value; }
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
            m_patternType_ = "";
            m_enabled_ = false;
            m_mode_ = "regex";
            m_patternText_ = "";
            m_updatedAt_ = default;
        }
    }
}
