using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 사용자 수동 저장 이미지 — 영구 vec store.
    /// 채팅에 표시된 이미지를 사용자가 Save 버튼으로 명시적으로 저장.
    /// 코멘트 텍스트로 임베딩 → 다음에 비슷한 상황(유사 코멘트)이면 자동 검색/재사용.
    ///
    /// 기존 vec 시스템(AB_Vec_Store/AB_Vec_Config) 과 완전히 분리된 별도 테이블.
    /// </summary>
    [Table("saved_images")]
    public class AB_Saved_Image_Model
    {
        /// <summary>레코드 PK.</summary>
        private long m_id_;
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>사용자 코멘트 (임베딩 소스). 비어있지 않아야 의미 있음.</summary>
        private string m_comment_ = "";
        [Column("comment")]
        public string Comment_
        {
            get { return m_comment_; }
            set { m_comment_ = value; }
        }

        /// <summary>이미지 데이터 (data URI 또는 URL string). 원본 그대로 보관.</summary>
        private string m_imageData_ = "";
        [Column("image_data")]
        public string ImageData_
        {
            get { return m_imageData_; }
            set { m_imageData_ = value; }
        }

        /// <summary>코멘트 텍스트 임베딩 벡터 (float[] → byte[] 직렬화).</summary>
        private byte[]? m_embedding_;
        [Column("embedding")]
        public byte[]? Embedding_
        {
            get { return m_embedding_; }
            set { m_embedding_ = value; }
        }

        /// <summary>임베딩에 사용된 모델 ID (어떤 모델의 벡터인지 추적).</summary>
        private string m_embeddingModel_ = "";
        [Column("embedding_model")]
        public string EmbeddingModel_
        {
            get { return m_embeddingModel_; }
            set { m_embeddingModel_ = value; }
        }

        /// <summary>임베딩 차원 수.</summary>
        private int m_dimensions_;
        [Column("dimensions")]
        public int Dimensions_
        {
            get { return m_dimensions_; }
            set { m_dimensions_ = value; }
        }

        /// <summary>최초 저장 시각 (UTC).</summary>
        private DateTime m_createdAt_ = DateTime.UtcNow;
        [Column("created_at")]
        public DateTime CreatedAt_
        {
            get { return m_createdAt_; }
            set { m_createdAt_ = value; }
        }

        /// <summary>마지막 자동 검색/사용 시각 (UTC).</summary>
        private DateTime m_lastUsedAt_ = DateTime.UtcNow;
        [Column("last_used_at")]
        public DateTime LastUsedAt_
        {
            get { return m_lastUsedAt_; }
            set { m_lastUsedAt_ = value; }
        }

        /// <summary>자동 검색으로 재사용된 횟수.</summary>
        private int m_useCount_;
        [Column("use_count")]
        public int UseCount_
        {
            get { return m_useCount_; }
            set { m_useCount_ = value; }
        }

        /// <summary>이 이미지를 원래 저장한 세션 ID (참조용). 0L=세션 외부.</summary>
        private long m_sourceSessionId_;
        [Column("source_session_id")]
        public long SourceSessionId_
        {
            get { return m_sourceSessionId_; }
            set { m_sourceSessionId_ = value; }
        }

        /// <summary>이 이미지를 원래 저장한 메시지 ID (참조용).</summary>
        private long m_sourceMessageId_;
        [Column("source_message_id")]
        public long SourceMessageId_
        {
            get { return m_sourceMessageId_; }
            set { m_sourceMessageId_ = value; }
        }
    }
}
