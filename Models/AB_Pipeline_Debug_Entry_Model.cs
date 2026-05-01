using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 파이프라인 디버그 엔트리. 스냅샷/트레이스/요청덤프/컨텍스트를 세션 단위로 기록.
    /// 메모리에 축적하지 않고 DB에 배치 기록 후 페이징 조회.
    /// </summary>
    [Table("pipeline_debug_entries")]
    public class AB_Pipeline_Debug_Entry_Model
    {
        private long m_id;
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id_
        {
            get { return m_id; }
            set { m_id = value; }
        }

        private string m_sessionId = "";
        [Column("session_id")]
        public string SessionId_
        {
            get { return m_sessionId; }
            set { m_sessionId = value; }
        }

        /// <summary>엔트리 타입: Snapshot, Trace, Request, Context</summary>
        private string m_entryType = "";
        [Column("entry_type")]
        public string EntryType_
        {
            get { return m_entryType; }
            set { m_entryType = value; }
        }

        private string m_nodeId = "";
        [Column("node_id")]
        public string NodeId_
        {
            get { return m_nodeId; }
            set { m_nodeId = value; }
        }

        private string m_label = "";
        [Column("label")]
        public string Label_
        {
            get { return m_label; }
            set { m_label = value; }
        }

        private DateTime m_timestampUtc = DateTime.UtcNow;
        [Column("timestamp_utc")]
        public DateTime TimestampUtc_
        {
            get { return m_timestampUtc; }
            set { m_timestampUtc = value; }
        }

        /// <summary>JSON 직렬화된 페이로드 (스냅샷/트레이스/요청덤프/컨텍스트 스택).</summary>
        private string m_payload = "";
        [Column("payload")]
        public string Payload_
        {
            get { return m_payload; }
            set { m_payload = value; }
        }

        /// <summary>페이로드 바이트 크기 (디시리얼라이즈 없이 UI 표시용).</summary>
        private int m_payloadSize;
        [Column("payload_size")]
        public int PayloadSize_
        {
            get { return m_payloadSize; }
            set { m_payloadSize = value; }
        }

        /// <summary>틱 번호 (Snapshot 타입용).</summary>
        private int m_tick;
        [Column("tick")]
        public int Tick_
        {
            get { return m_tick; }
            set { m_tick = value; }
        }

        /// <summary>에러 메시지 (있을 때만).</summary>
        private string? m_error;
        [Column("error")]
        public string? Error_
        {
            get { return m_error; }
            set { m_error = value; }
        }
    }
}
