using System.Collections.Generic;
using ArtificialBuilder.Models;

namespace ArtificialBuilder
{
    /// <summary>(typed-id-edp-rebase chunk 4v) DEBUG_ENTRIES_QUERY 결과 typed payload. SessionId_ 식별 + List_ + 조회 메타 (EntryType / Offset / Limit). AB_Pipeline_Debug_Entry_Model (DB Entity) 참조 = DB 모듈 위치 (Common 끌어올림 X — EF entity 는 DB 단일 보유).</summary>
    public sealed class AB_Debug_Entries_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        public AB_Session_Id SessionId_ = new(0L);
        public string? EntryType_;
        public int Offset_;
        public int Limit_;
        public List<AB_Pipeline_Debug_Entry_Model> List_ = new();

        public override void Reset()
        {
            base.Reset();
            SessionId_ = new(0L);
            EntryType_ = null;
            Offset_ = 0;
            Limit_ = 0;
            List_ = new();
        }
    }
}
