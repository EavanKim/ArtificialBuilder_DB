using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// Context Storage 1 row = 1 회 파이프라인 실행 단위.
    /// 4 계층 저장소의 3 번. 같은 turn 슬롯의 refresh 변종이 N 개면 row 가 N 개 — 모두 같은 turn_id 공유.
    /// 변종 중 1 개만 Session Storage 의 active_context_id 가 가리킴 (히스토리 활성).
    ///
    /// 노드 키 리스트는 별도 컬럼 X. Node Storage 가 context_id back-ref 보유 — `WHERE context_id = X` 로 조회.
    ///
    /// [[storage-layers]] / [[storage-layer-roles]] / [[refresh-as-branch]] 준수:
    /// - PK = id 단일. 샤드 키
    /// - turn_id 는 단일 키 참조 (Session Storage 가리킴)
    /// - 페이로드 / 노드 정보 보관 X. 실행 단위 메타만
    /// </summary>
    [Table("context_storage")]
    public class AB_Context_Storage_Model
    {
        /// <summary>context 단일 PK. 외부 참조 (Session.active_context_id, Node.context_id) 는 이 값.</summary>
        private long m_id_;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>소속 채팅 세션 ID. 인덱스 / 정리용 denormalize.</summary>
        private long m_sessionId_;
        [Required]
        [Column("session_id")]
        public long SessionId_
        {
            get { return m_sessionId_; }
            set { m_sessionId_ = value; }
        }

        /// <summary>이 context 가 속한 turn 슬롯 단일 키 (Session Storage row 가리킴).</summary>
        private long m_turnId_;
        [Column("turn_id")]
        public long TurnId_
        {
            get { return m_turnId_; }
            set { m_turnId_ = value; }
        }

        /// <summary>실행 시작 시각 (UTC). 변종 정렬 / 디버그용.</summary>
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
