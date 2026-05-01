using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>채팅 세션 엔티티 (PersonaDb의 chat_sessions 테이블 매핑)</summary>
    [Table("chat_sessions")]
    public class AB_Chat_Session_Model
    {
        /// <summary>세션 GUID</summary>
        private string m_id_ = Guid.NewGuid().ToString();
        [Key]
        [Column("id")]
        public string Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>소속 페르소나 ID</summary>
        private string m_personaId_ = "";
        [Required]
        [Column("persona_id")]
        public string PersonaId_
        {
            get { return m_personaId_; }
            set { m_personaId_ = value; }
        }

        /// <summary>사용 Circuit 이름</summary>
        private string m_circuitName_ = "";
        [Column("circuit_name")]
        public string CircuitName_
        {
            get { return m_circuitName_; }
            set { m_circuitName_ = value; }
        }

        /// <summary>세션 제목</summary>
        private string m_title_ = "새 채팅";
        [Column("title")]
        public string Title_
        {
            get { return m_title_; }
            set { m_title_ = value; }
        }

        /// <summary>생성 시각 (UTC)</summary>
        private DateTime m_createdAt_ = DateTime.UtcNow;
        [Column("created_at")]
        public DateTime CreatedAt_
        {
            get { return m_createdAt_; }
            set { m_createdAt_ = value; }
        }

        /// <summary>마지막 갱신 시각 (UTC)</summary>
        private DateTime m_updatedAt_ = DateTime.UtcNow;
        [Column("updated_at")]
        public DateTime UpdatedAt_
        {
            get { return m_updatedAt_; }
            set { m_updatedAt_ = value; }
        }

        /// <summary>컨텍스트 샤드 턴 블록 크기. 파일 샤드는 block = turn_index / TurnShardSize 로 분할. 변경 시 해당 세션 데이터 wipe.</summary>
        private int m_turnShardSize_ = 50;
        [Column("turn_shard_size")]
        public int TurnShardSize_
        {
            get { return m_turnShardSize_; }
            set { m_turnShardSize_ = value; }
        }

    }
}
