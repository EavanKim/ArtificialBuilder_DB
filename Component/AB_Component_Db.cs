using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Component
{
    // DB 매개 작업 카테고리 추상 베이스. 후손 = AB_Component_Db/ sub-folder (Read / Write / Delete).
    public abstract class AB_Component_Db : AB_Component
    {
        protected AB_Component_Db()
        {
        }
    }
}
