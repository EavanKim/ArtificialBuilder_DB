using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<AB_Object_DB_App_Model?> 콘크리트 — App.Model 단일 entity slot.
    // MODEL_GET 결과 슬롯 (miss = null) + MODEL_ADD / MODEL_SAVE 입력 슬롯.
    public class AB_Data_DB_App_Model : AB_Data<AB_Object_DB_App_Model?>
    {
        public AB_Data_DB_App_Model() : base(null)
        {
        }
    }
}
