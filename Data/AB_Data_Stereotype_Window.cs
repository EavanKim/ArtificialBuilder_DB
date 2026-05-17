using System.Collections.Generic;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<List<AB_Object_DB_Node_Row>> 콘크리트 — 단순형 stereotype sliding window 결과 slot.
    // storage-policy 2026-05-17 § "Active Result 추적 정책" 단순형 stereotype 정합.
    // BuildWindow_ 호출 후 N (~10) turn backward chain 매개 active result row 묶음 (최근 → 과거 순서). 부분 묶음 (PrevTurnId == null 매개 chain 종결) 허용.
    // caller = AI provider Object 매개 prompt 구성 시점 본 slot read.
    public class AB_Data_Stereotype_Window : AB_Data<List<AB_Object_DB_Node_Row>>
    {
        public AB_Data_Stereotype_Window() : base(new List<AB_Object_DB_Node_Row>())
        {
        }
    }
}
