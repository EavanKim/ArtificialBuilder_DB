using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// Session Storage 1 row = 1 turn 슬롯.
    /// 4 계층 저장소의 2 번. 한 채팅 세션의 turn 시퀀스를 1 직선 linked list 로 보관.
    ///
    /// 한 row 가 한 turn 슬롯 (UI 상 1 줄). 같은 turn 의 refresh 변종 (Context Storage row N 개) 중
    /// `active_context_id` 가 가리키는 1 개만 LLM 히스토리 활성. 나머지는 storage 보존되되 히스토리 제외.
    ///
    /// [[storage-layers]] / [[refresh-as-branch]] 준수:
    /// - PK = id 단일. max(turn_index)+1 산술 금지 — 추가 / 삭제는 prev/next 포인터 list 연산
    /// - input_resource_id 는 turn 단위. 같은 turn 의 모든 context 가 공유. 새로고침 시 input 재생성 X
    /// - 0 번 row (head) 는 시작 노드 정보 (Circuit greeting) — input/active 모두 null 가능
    /// </summary>
    [Table("session_storage")]
    public class AB_Session_Storage_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        /// <summary>turn 슬롯 단일 PK. 외부 참조 (Context.turn_id) 는 이 값.</summary>
        private long m_id_;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>소속 채팅 세션 ID. 소프트웨어 FK → chat_sessions.id (cascade delete).</summary>
        private long m_sessionId_;
        [Required]
        [Column("session_id")]
        public long SessionId_
        {
            get { return m_sessionId_; }
            set { m_sessionId_ = value; }
        }

        /// <summary>linked list 이전 슬롯 id. head 면 null.</summary>
        private long? m_prevId_;
        [Column("prev_id")]
        public long? PrevId_
        {
            get { return m_prevId_; }
            set { m_prevId_ = value; }
        }

        /// <summary>linked list 다음 슬롯 id. tail 이면 null.</summary>
        private long? m_nextId_;
        [Column("next_id")]
        public long? NextId_
        {
            get { return m_nextId_; }
            set { m_nextId_ = value; }
        }

        /// <summary>
        /// 이 turn 의 사용자 입력 페이로드. Resource Storage 단일 키.
        /// 같은 turn 의 모든 context 가 공유. 새로고침해도 변경 안 됨.
        /// 0 번 row (시작 슬롯) 는 null.
        /// </summary>
        private long? m_inputResourceId_;
        [Column("input_resource_id")]
        public long? InputResourceId_
        {
            get { return m_inputResourceId_; }
            set { m_inputResourceId_ = value; }
        }

        /// <summary>
        /// 현재 히스토리 활성 context 단일 키. ◀▶ 네비로 변경.
        /// null 이면 아직 실행 안 됨 (또는 시작 슬롯).
        /// </summary>
        private long? m_activeContextId_;
        [Column("active_context_id")]
        public long? ActiveContextId_
        {
            get { return m_activeContextId_; }
            set { m_activeContextId_ = value; }
        }

        /// <summary>슬롯 생성 시각 (UTC).</summary>
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
