using System.Collections.Generic;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<List<AB_Object_DB_Turn_Row>> 콘크리트 — turn_id range query 결과 list slot.
    // FIND_RANGE 결과 슬롯. 초기 = 빈 list.
    public class AB_Data_DB_Turn_List : AB_Data<List<AB_Object_DB_Turn_Row>>
    {
        public AB_Data_DB_Turn_List() : base(new List<AB_Object_DB_Turn_Row>())
        {
        }
    }
}
