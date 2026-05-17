using ArtificialBuilder.DB.Object;
using Microsoft.EntityFrameworkCore;

namespace ArtificialBuilder.DB
{
    // Result 노드별 키 샤딩 DB DbContext — 노드 폴더 안 1 파일 (`{data_key_range_100}.db`) 매개 1 instance.
    // storage-policy 2026-05-17 round 2 § "Result 노드별 키 샤딩 DB 스키마" 정합. AB_Object_DB_Node_Row 단일 DbSet.
    // EDP_Db_Engine.OpenDatabase<AB_Context_DB_Node_Shard>(file_path, _opts => new AB_Context_DB_Node_Shard(_opts)) 매개 instance.
    // FK 최저화 룰 — HasIndex OK / HasForeignKey X (TurnId = plain column).
    internal sealed class AB_Context_DB_Node_Shard : DbContext
    {
        public DbSet<AB_Object_DB_Node_Row> NodeRows { get; set; } = null!;

        public AB_Context_DB_Node_Shard(DbContextOptions<AB_Context_DB_Node_Shard> _options) : base(_options)
        {
        }

        protected override void OnModelCreating(ModelBuilder _builder)
        {
            // PK = DataKey 단일 ([[feedback_key_single]] 정합).
            // index = (TurnId, ResultSeq) — bucket 매개 row 조회 query.
            // circuit_id / node_number 컬럼 X — 폴더명 매개 식별 (파일 자체가 노드 단위).
            _builder.Entity<AB_Object_DB_Node_Row>(_e =>
            {
                _e.HasKey(_x => _x.DataKey);
                _e.HasIndex(_x => new { _x.TurnId, _x.ResultSeq });
                // 13r4b 매개 IsActive 인덱스 — FIND_SIMILAR WHERE 매개 활성 row 매개 filter 매개 fast path.
                _e.HasIndex(_x => _x.IsActive);
            });
        }
    }
}
