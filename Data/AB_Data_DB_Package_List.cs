using System.Collections.Generic;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<List<AB_Object_DB_Package>> 콘크리트 — Package 카탈로그 list slot.
    // INFO_GET_ALL 결과 슬롯. 초기 = 빈 list.
    public class AB_Data_DB_Package_List : AB_Data<List<AB_Object_DB_Package>>
    {
        public AB_Data_DB_Package_List() : base(new List<AB_Object_DB_Package>())
        {
        }
    }
}
