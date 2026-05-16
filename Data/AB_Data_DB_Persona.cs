using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<AB_Object_DB_Persona?> 콘크리트 — Persona 단일 entity slot.
    // PERSONA_DB_LOAD_ACTIVE 결과 슬롯 (is_active = true row 1, miss = null).
    public class AB_Data_DB_Persona : AB_Data<AB_Object_DB_Persona?>
    {
        public AB_Data_DB_Persona() : base(null)
        {
        }
    }
}
