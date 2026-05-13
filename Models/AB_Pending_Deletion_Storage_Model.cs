using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 4 계층 저장소 cascade 삭제 대기 큐.
    /// 분리 정책: 큰 subtree (Session_Storage / Context_Storage) 삭제 요청은 즉시 cascade 안 하고
    /// 이 테이블에 root 만 enqueue → 백그라운드 sweeper 가 매 틱 leaf-first 로 1 건씩 정리.
    ///
    /// 알고리즘 (leaf-first time-budgeted sweep):
    ///  1. 가장 오래된 pending row 1 건 획득
    ///  2. target_table 의 children 한 건 leaf 까지 walk
    ///  3. leaf 삭제 (resource → node → context → slot 순)
    ///  4. parent linked list 포인터 정리 (Session_Storage 의 prev/next)
    ///  5. 더 이상 children 없으면 root 자체 삭제 + pending row pop
    ///  6. 매 틱 budget 다 쓰면 다음 틱 양보
    /// </summary>
    [Table("pending_deletions")]
    public class AB_Pending_Deletion_Storage_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>대상 테이블명 — "session_storage" / "context_storage". leaf (node / resource) 는 enqueue 안 함 (직접 delete).</summary>
        private string m_targetTable_ = "";
        [Required]
        [Column("target_table")]
        public string TargetTable_
        {
            get { return m_targetTable_; }
            set { m_targetTable_ = value; }
        }

        /// <summary>대상 row id.</summary>
        private long m_targetId_;
        [Column("target_id")]
        public long TargetId_
        {
            get { return m_targetId_; }
            set { m_targetId_ = value; }
        }

        /// <summary>enqueue 시각 (FIFO 정렬용, UTC).</summary>
        private DateTime m_enqueuedAt_ = DateTime.UtcNow;
        [Column("enqueued_at")]
        public DateTime EnqueuedAt_
        {
            get { return m_enqueuedAt_; }
            set { m_enqueuedAt_ = value; }
        }
    }
}
