using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>로컬 GGUF 모델 entity — typed-id-edp-rebase sub 4 chunk 4o entity 화. filesystem 파일 (.gguf) 에 1:1 매핑되는 DB row. PK = long (AB_Model_Id), file_name = entity attribute. AB_Local_Model_Manager 가 scan-on-load 동기화 (filesystem → DB upsert).</summary>
    [Table("llama_models")]
    public class AB_Llama_Model
    {
        private long m_id_ = ArtificialBuilder.AB_Id_Issuer.Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string m_fileName_ = "";
        [Required]
        [Column("file_name")]
        public string FileName_
        {
            get { return m_fileName_; }
            set { m_fileName_ = value; }
        }

        private string m_filePath_ = "";
        [Required]
        [Column("file_path")]
        public string FilePath_
        {
            get { return m_filePath_; }
            set { m_filePath_ = value; }
        }

        private long m_fileSizeBytes_;
        [Column("file_size_bytes")]
        public long FileSizeBytes_
        {
            get { return m_fileSizeBytes_; }
            set { m_fileSizeBytes_ = value; }
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
