using ArtificialBuilder.DB.Component;

namespace ArtificialBuilder.DB.Object
{
    // 콘크리트 Object — 폴더 + 시계열 매개 파일 N (File_Sharding) + 시계열 매개 핸들 라우팅 CRUD (CRUD_Sharding_History).
    // Turn 히스토리 (storage-policy 시계열 샤딩 — `runtime/turn/{circuit_id}/{turn_bucket}.db`).
    // canon § "Object / Component 다형성 조립 패턴" 정합.
    public class AB_Object_DB_Sharding_History : AB_Object_DB
    {
        public AB_Object_DB_Sharding_History()
        {
            m_file = new AB_Component_DB_File_Sharding();
            m_crud = new AB_Component_DB_CRUD_Sharding_History();
        }
    }
}
