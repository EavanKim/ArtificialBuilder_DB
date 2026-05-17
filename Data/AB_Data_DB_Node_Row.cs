using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<AB_Object_DB_Node_Row?> 콘크리트 — Result 1 row slot.
    // GET_BY_KEY 결과 슬롯 (miss = null). PUT 입력 = 본 slot 매개 entity 운반.
    public class AB_Data_DB_Node_Row : AB_Data<AB_Object_DB_Node_Row?>
    {
        public AB_Data_DB_Node_Row() : base(null)
        {
        }
    }
}
