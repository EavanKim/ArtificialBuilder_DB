using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<AB_Object_DB_Package?> 콘크리트 — Package 단일 entity slot.
    // INFO_GET 결과 슬롯 (miss = null) + INFO_ADD / INFO_SAVE 입력 슬롯.
    public class AB_Data_DB_Package : AB_Data<AB_Object_DB_Package?>
    {
        public AB_Data_DB_Package() : base(null)
        {
        }
    }
}
