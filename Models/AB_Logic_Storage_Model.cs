using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// Node Storage 1 row = 1 그래프 노드의 1 회 실행 결과.
    /// 4 계층 저장소의 4 번. context 안에서 어떤 그래프 노드가 무엇을 출력했는지 기록.
    /// 페이로드는 들고있지 않음 — Resource Storage 단일 키만 (resource_id) 참조.
    ///
    /// [[storage-layers]] / [[storage-layer-roles]] / [[key-single]] 준수:
    /// - PK = id 단일. 샤드 키
    /// - context_id / resource_id 는 단일 키 참조
    /// - node_id 는 그래프 노드 식별자 문자열 (DB row FK 아님 — 그래프의 논리 식별자)
    /// - emission_order 는 같은 (context, node) 안의 발행 순서 컬럼
    /// - 페이로드 inline 금지 — resource_id 로만 가리킴
    /// </summary>
    [Table("logic_storage")]
    public class AB_Logic_Storage_Model
    {
        /// <summary>로직 실행 row 단일 PK.</summary>
        private long m_id_;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>이 실행이 속한 context 단일 키 (Context Storage 가리킴).</summary>
        private long m_contextId_;
        [Column("context_id")]
        public long ContextId_
        {
            get { return m_contextId_; }
            set { m_contextId_ = value; }
        }

        /// <summary>그래프 노드 식별자 (UI 노드 그래프의 논리 ID 문자열). DB row FK 아님.</summary>
        private string m_nodeId_ = "";
        [Required]
        [Column("logic_id")]
        public string NodeId_
        {
            get { return m_nodeId_; }
            set { m_nodeId_ = value; }
        }

        /// <summary>같은 (context, node) 안의 발행 순서 (0 부터). 한 노드가 여러 출력을 낸 경우 구분.</summary>
        private int m_emissionOrder_;
        [Column("emission_order")]
        public int EmissionOrder_
        {
            get { return m_emissionOrder_; }
            set { m_emissionOrder_ = value; }
        }

        /// <summary>출력 페이로드 단일 키 (Resource Storage 가리킴). 출력이 없으면 null.</summary>
        private long? m_resourceId_;
        [Column("resource_id")]
        public long? ResourceId_
        {
            get { return m_resourceId_; }
            set { m_resourceId_ = value; }
        }

        /// <summary>실행 메타 JSON (tokens / cost / role / trace 등). null 가능.</summary>
        private string? m_metaJson_;
        [Column("meta_json")]
        public string? MetaJson_
        {
            get { return m_metaJson_; }
            set { m_metaJson_ = value; }
        }

        /// <summary>실행 시각 (UTC). 디버그 / 정렬용.</summary>
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
    }
}
