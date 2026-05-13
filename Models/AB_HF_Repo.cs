using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>HuggingFace repo entity — typed-id-edp-rebase sub 4 chunk 4q entity 화. repo_id ("owner/repo" string) = entity attribute (HF 외부 식별자), PK = long. 캐시 metadata (description / license / readme_snippet).</summary>
    [Table("hf_repos")]
    public class AB_HF_Repo
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string m_repoId_ = "";
        [Required]
        [Column("repo_id")]
        public string RepoId_
        {
            get { return m_repoId_; }
            set { m_repoId_ = value; }
        }

        private string m_description_ = "";
        [Column("description")]
        public string Description_
        {
            get { return m_description_; }
            set { m_description_ = value; }
        }

        private string m_license_ = "";
        [Column("license")]
        public string License_
        {
            get { return m_license_; }
            set { m_license_ = value; }
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
