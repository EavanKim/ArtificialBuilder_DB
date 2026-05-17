using System.Collections.Generic;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<List<AB_Object_DB_Node_Row>> 콘크리트 — Result list slot.
    // FIND_BY_TURN / FIND_BY_NODE 결과 슬롯. 초기 = 빈 list.
    public class AB_Data_DB_Node_List : AB_Data<List<AB_Object_DB_Node_Row>>
    {
        public AB_Data_DB_Node_List() : base(new List<AB_Object_DB_Node_Row>())
        {
        }
    }
}
