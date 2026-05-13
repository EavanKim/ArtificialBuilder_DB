using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// Resource Storage 1 row — 페이로드 KV.
    /// 4 계층 저장소의 1 번. 생성된 모든 결과물 (텍스트 / 이미지 / 사운드 / 모델 등) 의 단일 종착지.
    /// 다른 어떤 저장소도 가리키지 않는 끝 노드. KV — 관계 없음.
    ///
    /// [[storage-layers]] / [[key-single]] 준수:
    /// - PK = id 단일 컬럼. 합성 / 인코딩 키 금지
    /// - 메타 (kind / size / created_at) 는 컬럼
    /// - 페이로드는 작은 건 inline, 큰 건 외부 파일 path 만
    /// </summary>
    [Table("resource_storage")]
    public class AB_Resource_Storage_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        /// <summary>리소스 단일 PK. 외부 참조 (Node.resource_id, Session.input_resource_id) 는 이 값.</summary>
        private long m_id_;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>페이로드 종류 — "text" / "image" / "audio" / "model" / "embedding" 등.</summary>
        private string m_kind_ = "text";
        [Required]
        [Column("kind")]
        public string Kind_
        {
            get { return m_kind_; }
            set { m_kind_ = value; }
        }

        /// <summary>페이로드 바이트 크기. 샤딩 / 메모리 캐시 정책 결정 기준.</summary>
        private long m_size_;
        [Column("size")]
        public long Size_
        {
            get { return m_size_; }
            set { m_size_ = value; }
        }

        /// <summary>작은 페이로드 inline 저장 (텍스트 / data URI 등). 큰 페이로드는 null 두고 path 사용.</summary>
        private string? m_payloadInline_;
        [Column("payload_inline")]
        public string? PayloadInline_
        {
            get { return m_payloadInline_; }
            set { m_payloadInline_ = value; }
        }

        /// <summary>큰 페이로드 외부 파일 경로. inline 사용 시 null.</summary>
        private string? m_payloadPath_;
        [Column("payload_path")]
        public string? PayloadPath_
        {
            get { return m_payloadPath_; }
            set { m_payloadPath_ = value; }
        }

        /// <summary>최초 생성 시각 (UTC).</summary>
        private DateTime m_createdAt_ = DateTime.UtcNow;
        [Column("created_at")]
        public DateTime CreatedAt_
        {
            get { return m_createdAt_; }
            set { m_createdAt_ = value; }
        }

        /// <summary>mark-sweep 삭제 플래그. true 면 read 경로 invisible + sweeper 가 leaf-first DELETE 대상.</summary>
        private bool m_isDeleted_;
        [Column("is_deleted")]
        public bool IsDeleted_
        {
            get { return m_isDeleted_; }
            set { m_isDeleted_ = value; }
        }

        public override void Reset()
        {
            base.Reset();
            m_id_ = 0L;
            m_kind_ = "text";
            m_size_ = 0L;
            m_payloadInline_ = null;
            m_payloadPath_ = null;
            m_createdAt_ = default;
            m_isDeleted_ = false;
        }
    }
}
