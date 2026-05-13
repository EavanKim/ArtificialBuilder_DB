using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>Logic 호출 history (turn = 로직 히스토리 동의어). turn id / 입력 / 출력 / timestamp 메타. plan: docs/plans/doing/db-three-way-split/1-logic-db-schema.md</summary>
    [Table("logic_history_turns")]
    public class AB_Logic_History_Turn_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string m_turnId_ = "";
        [Required]
        [Column("turn_id")]
        public string TurnId_
        {
            get { return m_turnId_; }
            set { m_turnId_ = value; }
        }

        private int m_turnIndex_ = 0;
        [Column("turn_index")]
        public int TurnIndex_
        {
            get { return m_turnIndex_; }
            set { m_turnIndex_ = value; }
        }

        private string? m_inputSummary_;
        [Column("input_summary")]
        public string? InputSummary_
        {
            get { return m_inputSummary_; }
            set { m_inputSummary_ = value; }
        }

        private string? m_outputSummary_;
        [Column("output_summary")]
        public string? OutputSummary_
        {
            get { return m_outputSummary_; }
            set { m_outputSummary_ = value; }
        }

        private string? m_resultStatus_;
        [Column("result_status")]
        public string? ResultStatus_
        {
            get { return m_resultStatus_; }
            set { m_resultStatus_ = value; }
        }

        private DateTime m_startedAt_ = DateTime.UtcNow;
        [Column("started_at")]
        public DateTime StartedAt_
        {
            get { return m_startedAt_; }
            set { m_startedAt_ = value; }
        }

        private DateTime m_endedAt_ = DateTime.UtcNow;
        [Column("ended_at")]
        public DateTime EndedAt_
        {
            get { return m_endedAt_; }
            set { m_endedAt_ = value; }
        }

        public override void Reset()
        {
            base.Reset();
            m_id_ = 0L;
            m_turnId_ = "";
            m_turnIndex_ = 0;
            m_inputSummary_ = null;
            m_outputSummary_ = null;
            m_resultStatus_ = null;
            m_startedAt_ = default;
            m_endedAt_ = default;
        }
    }
}
