using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>HF 다운로드 큐 항목 entity — typed-id-edp-rebase sub 4 chunk 4p entity 화. 기존 itemId (GUID string) 폐기 + long PK 부여. repo_id / file_name 등 = entity attribute.</summary>
    [Table("hf_downloads")]
    public class AB_HF_Download : ArtificialBuilder_EDP.Core.AB_Object
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

        private string m_fileName_ = "";
        [Required]
        [Column("file_name")]
        public string FileName_
        {
            get { return m_fileName_; }
            set { m_fileName_ = value; }
        }

        private string m_modelName_ = "";
        [Column("model_name")]
        public string ModelName_
        {
            get { return m_modelName_; }
            set { m_modelName_ = value; }
        }

        private long m_totalBytes_;
        [Column("total_bytes")]
        public long TotalBytes_
        {
            get { return m_totalBytes_; }
            set { m_totalBytes_ = value; }
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
