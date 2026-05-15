using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // DB 도메인 카테고리 추상 베이스. 후손 = AB_Object_Db/ sub-folder (App / Persona / Circuit / Logic / Response_Ui).
    // attach 권장 Component = Db_Read + Db_Write + Db_Delete cascade.
    // Dispose / Tick 본체 = 후손 콘크리트 의무 (추상 단계 본체 X).
    public abstract class AB_Object_Db : AB_Object
    {
        protected AB_Object_Db()
        {
        }
    }
}
