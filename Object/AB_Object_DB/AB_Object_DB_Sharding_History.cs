using ArtificialBuilder.DB.Component;

namespace ArtificialBuilder.DB.Object
{
    // 콘크리트 Object — 폴더 + 시계열 매개 파일 N (File_Sharding) + 시계열 매개 라우팅 CRUD (CRUD_Sharding_History).
    // Turn 시계열 샤딩 (storage-policy 2026-05-17 round 2 — `runtime/turn/{circuit_id}/{turn_id_range_100}.db`).
    // canon § "Object / Component 다형성 조립 패턴" 정합.
    // shard_key = turn_id / 100 (caller 매개 산정 후 BeginTransactionAsync_(shard_key) 호출).
    public class AB_Object_DB_Sharding_History : AB_Object_DB
    {
        public AB_Object_DB_Sharding_History()
        {
            m_file = new AB_Component_DB_File_Sharding();
            m_crud = new AB_Component_DB_CRUD_Sharding_History();
            // D6=A — TContext = AB_Context_DB_Turn_Shard (Turn 메타 단일 DbSet). 콘크리트 ctor 안 결정.
            m_opener = (_engine, _file_path) => _engine.OpenDatabase<AB_Context_DB_Turn_Shard>(_file_path, _opts => new AB_Context_DB_Turn_Shard(_opts));
        }
    }
}
