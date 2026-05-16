using ArtificialBuilder.DB.Object;
using Microsoft.EntityFrameworkCore;

namespace ArtificialBuilder.DB
{
    // 키 샤딩 DB DbContext — 노드 폴더 안 1 파일 (`{shard_key}.db`) 매개 1 instance.
    // storage-policy 2026-05-17 § "키 샤딩 DB 스키마" 정합. AB_Object_DB_Node_Row 단일 DbSet.
    // EDP_Db_Engine.OpenDatabase<AB_Context_DB_Node_Shard>(file_path, _opts => new AB_Context_DB_Node_Shard(_opts)) 매개 instance.
    // round 1 (skeleton) = DbSet + OnModelCreating PK 만. Component_DB_File_Sharding 본체 = round 2.
    internal sealed class AB_Context_DB_Node_Shard : DbContext
    {
        public DbSet<AB_Object_DB_Node_Row> NodeRows { get; set; } = null!;

        public AB_Context_DB_Node_Shard(DbContextOptions<AB_Context_DB_Node_Shard> _options) : base(_options)
        {
        }

        protected override void OnModelCreating(ModelBuilder _builder)
        {
            // PK = (TurnId, LogicInvocationId) 복합 키 — 같은 (turn, logic_invocation) 매개 단일 페이로드 row.
            // node_id 컬럼 X — 폴더명 매개 식별 (파일 자체가 노드 단위).
            _builder.Entity<AB_Object_DB_Node_Row>(_e =>
            {
                _e.HasKey(_x => new { _x.TurnId, _x.LogicInvocationId });
            });
        }
    }
}
