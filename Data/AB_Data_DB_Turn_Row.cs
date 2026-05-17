using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<AB_Object_DB_Turn_Row?> 콘크리트 — Turn bucket 1 row slot.
    // GET_BY_ID 결과 슬롯 (miss = null). CREATE_BUCKET / APPEND_RESULT / SET_SELECTED_INDEX 입력 = 본 slot 매개 entity 운반.
    public class AB_Data_DB_Turn_Row : AB_Data<AB_Object_DB_Turn_Row?>
    {
        public AB_Data_DB_Turn_Row() : base(null)
        {
        }
    }
}
