using ArtificialBuilder;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>Circuit 내장 에셋 (이미지/사운드/폰트 등 바이너리). (example-mental-restructure Phase B Sub 4 Circuit 7/8) — string PK → long PK.</summary>
    [Table("circuit_assets")]
    public class AB_Circuit_Asset_Model : ArtificialBuilder_EDP.Core.AB_Object
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

        private string m_fileName_ = "";
        [Column("file_name")]
        public string FileName_
        {
            get { return m_fileName_; }
            set { m_fileName_ = value; }
        }

        private string m_assetType_ = "image";
        [Column("asset_type")]
        public string AssetType_
        {
            get { return m_assetType_; }
            set { m_assetType_ = value; }
        }

        private string m_mimeType_ = "";
        [Column("mime_type")]
        public string MimeType_
        {
            get { return m_mimeType_; }
            set { m_mimeType_ = value; }
        }

        private string m_loadMode_ = "ondemand";
        [Column("load_mode")]
        public string LoadMode_
        {
            get { return m_loadMode_; }
            set { m_loadMode_ = value; }
        }

        private string m_folderPath_ = "";
        [Column("folder_path")]
        public string FolderPath_
        {
            get { return m_folderPath_; }
            set { m_folderPath_ = value; }
        }

        private byte[] m_data_ = Array.Empty<byte>();
        [Column("data")]
        public byte[] Data_
        {
            get { return m_data_; }
            set { m_data_ = value; }
        }

        private long m_fileSize_;
        [Column("file_size")]
        public long FileSize_
        {
            get { return m_fileSize_; }
            set { m_fileSize_ = value; }
        }

        private DateTime m_createdAt_ = DateTime.UtcNow;
        [Column("created_at")]
        public DateTime CreatedAt_
        {
            get { return m_createdAt_; }
            set { m_createdAt_ = value; }
        }

        [NotMapped]
        public string LeafName_ => string.IsNullOrEmpty(Name_) ? FileName_ : System.IO.Path.GetFileName(Name_);

        public override void Reset()
        {
            base.Reset();
            m_id_ = 0L;
            m_name_ = "";
            m_fileName_ = "";
            m_assetType_ = "image";
            m_mimeType_ = "";
            m_loadMode_ = "ondemand";
            m_folderPath_ = "";
            m_data_ = Array.Empty<byte>();
            m_fileSize_ = 0L;
            m_createdAt_ = default;
        }
    }
}
