using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>LLM 모델 연결 설정 (프로바이더/엔드포인트/키).</summary>
    [Table("model_configs")]
    public class AB_Model_Config_Model
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
        [Required]
        [Column("name")]
        public string Name_
        {
            get { return m_name_; }
            set { m_name_ = value; }
        }

        private string m_providerType_ = "";
        [Required]
        [Column("provider_type")]
        public string ProviderType_
        {
            get { return m_providerType_; }
            set { m_providerType_ = value; }
        }

        private string m_apiKeyEncrypted_ = "";
        [Column("api_key_encrypted")]
        public string ApiKeyEncrypted_
        {
            get { return m_apiKeyEncrypted_; }
            set { m_apiKeyEncrypted_ = value; }
        }

        private string m_endpointUrl_ = "";
        [Column("endpoint_url")]
        public string EndpointUrl_
        {
            get { return m_endpointUrl_; }
            set { m_endpointUrl_ = value; }
        }

        private string m_modelName_ = "";
        [Required]
        [Column("model_name")]
        public string ModelName_
        {
            get { return m_modelName_; }
            set { m_modelName_ = value; }
        }

        private string m_modelType_ = "chat";
        [Column("model_type")]
        public string ModelType_
        {
            get { return m_modelType_; }
            set { m_modelType_ = value; }
        }

        private uint m_contextSize_;
        [Column("context_size")]
        public uint ContextSize_
        {
            get { return m_contextSize_; }
            set { m_contextSize_ = value; }
        }

        private int m_gpuLayers_ = -1;
        [Column("gpu_layers")]
        public int GpuLayers_
        {
            get { return m_gpuLayers_; }
            set { m_gpuLayers_ = value; }
        }

        private uint m_batchSize_;
        [Column("batch_size")]
        public uint BatchSize_
        {
            get { return m_batchSize_; }
            set { m_batchSize_ = value; }
        }

        private uint m_seqMax_;
        [Column("seq_max")]
        public uint SeqMax_
        {
            get { return m_seqMax_; }
            set { m_seqMax_ = value; }
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
