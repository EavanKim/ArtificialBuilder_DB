using ArtificialBuilder.DB.Component;

namespace ArtificialBuilder.DB.Object
{
    // 콘크리트 Object — 단일 파일 lifecycle + 단일 handle CRUD. 에셋 DB (App / Persona / Package).
    // canon § "Object / Component 다형성 조립 패턴" 정합 — ctor 안 콘크리트 Component instance new + abstract field 매개 노출.
    // 외부 attach API X. 매니저는 본 콘크리트 인지 X — AB_Object_DB abstract 만 매개 동작.
    public class AB_Object_DB_Normal : AB_Object_DB
    {
        public AB_Object_DB_Normal()
        {
            m_file = new AB_Component_DB_File_Normal();
            m_crud = new AB_Component_DB_CRUD_Normal();
        }
    }
}
