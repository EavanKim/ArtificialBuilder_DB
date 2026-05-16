using System.Collections.Generic;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<List<AB_Object_DB_App_Model>> 콘크리트 — App.Model 카탈로그 list slot.
    // MODEL_GET_ALL 결과 슬롯. 초기 = 빈 list.
    public class AB_Data_DB_App_Model_List : AB_Data<List<AB_Object_DB_App_Model>>
    {
        public AB_Data_DB_App_Model_List() : base(new List<AB_Object_DB_App_Model>())
        {
        }
    }
}
