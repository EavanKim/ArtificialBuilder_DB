using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 파이프라인 템플릿. (현재 미사용 — 1:1 Circuit 임베디드 그래프로 전환)
    /// 라이브러리 UI 제거됨. EF 마이그레이션 회피용으로 클래스/DbSet만 유지.
    /// </summary>
    [Table("pipelines")]
    public class AB_Pipeline_Model
    {
        private long m_id_ = ArtificialBuilder.AB_Id_Issuer.Issue();
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

        /// <summary>설명 (선택)</summary>
        private string? m_description_;
        [Column("description")]
        public string? Description_
        {
            get { return m_description_; }
            set { m_description_ = value; }
        }

        /// <summary>직렬화된 노드 그래프 JSON (AB_Circuit.Serialize 결과)</summary>
        private string m_graphJson_ = "";
        [Column("graph_json")]
        public string GraphJson_
        {
            get { return m_graphJson_; }
            set { m_graphJson_ = value; }
        }

        /// <summary>Circuit 배포 시 포함된 내장 파이프라인 → 수정 불가</summary>
        private bool m_isBuiltin_ = false;
        [Column("is_builtin")]
        public bool IsBuiltin_
        {
            get { return m_isBuiltin_; }
            set { m_isBuiltin_ = value; }
        }

        /// <summary>원본 Circuit명 (배포 출처 추적용, null=유저 생성)</summary>
        private string? m_sourceCircuit_;
        [Column("source_circuit")]
        public string? SourceCircuit_
        {
            get { return m_sourceCircuit_; }
            set { m_sourceCircuit_ = value; }
        }

        /// <summary>노드 수 (UI 표시용 캐시)</summary>
        private int m_nodeCount_ = 0;
        [Column("node_count")]
        public int NodeCount_
        {
            get { return m_nodeCount_; }
            set { m_nodeCount_ = value; }
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
