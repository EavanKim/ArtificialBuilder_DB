using ArtificialBuilder.DB.Object;
using Microsoft.EntityFrameworkCore;

namespace ArtificialBuilder.DB
{
    // 시계열 샤딩 DB DbContext — 서킷 폴더 안 1 파일 (`{turn_bucket}.db`) 매개 1 instance.
    // storage-policy 2026-05-17 round 2 § "Turn 시계열 샤딩 DB 스키마" 정합. AB_Object_DB_Turn_Row 단일 DbSet.
    // FK 최저화 룰 — HasIndex OK / HasForeignKey X.
    internal sealed class AB_Context_DB_Turn_Shard : DbContext
    {
        public DbSet<AB_Object_DB_Turn_Row> TurnRows { get; set; } = null!;

        public AB_Context_DB_Turn_Shard(DbContextOptions<AB_Context_DB_Turn_Shard> _options) : base(_options)
        {
        }

        protected override void OnModelCreating(ModelBuilder _builder)
        {
            // PK = TurnId.
            // index = PrevTurnId — 시계열 traverse query 매개.
            // circuit_id 컬럼 X — 폴더명 매개 식별.
            _builder.Entity<AB_Object_DB_Turn_Row>(_e =>
            {
                _e.HasKey(_x => _x.TurnId);
                _e.HasIndex(_x => _x.PrevTurnId);
            });
        }
    }
}
