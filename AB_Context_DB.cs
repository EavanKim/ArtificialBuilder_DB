using ArtificialBuilder.DB.Object;
using Microsoft.EntityFrameworkCore;

namespace ArtificialBuilder.DB
{
    // AB_Manager_DB 단일 내부 DbContext. 외부 노출 X — internal sealed (AB_Manager_DB 만 instantiate).
    // 3 DB 도메인 (App / Persona / Package) entity DbSet 망라 — storage-policy 2026-05-16 정본 정합.
    //   App = Models (카탈로그) + Credentials (AES 암호화 비밀)
    //   Persona = Personas (최소집합)
    //   Package = Packages (최소집합)
    //   Circuit / Logic / UI 명령 = JSON 매체 매개 본 DbContext 제외.
    // MS Learn 정본: "DbContext instance represents a session with the database... Unit Of Work + Repository patterns".
    //   threading: "EF Core does not support multiple parallel operations being run on the same DbContext instance" — entry.Lock (SemaphoreSlim) 매개 직렬화.
    // 단일 인스턴스 매개 모든 entity 망라 — 도메인별 / sharding 분리 X (사용자 정본 2026-05-15).
    internal sealed class AB_Context_DB : DbContext
    {
        public DbSet<AB_Object_DB_App_Model> AppModels { get; set; } = null!;
        public DbSet<AB_Object_DB_App_Credential> AppCredentials { get; set; } = null!;
        public DbSet<AB_Object_DB_Persona> Personas { get; set; } = null!;
        public DbSet<AB_Object_DB_Package> Packages { get; set; } = null!;
        public DbSet<AB_Object_DB_Circuit> Circuits { get; set; } = null!;
        public DbSet<AB_Object_DB_Logic> Logics { get; set; } = null!;
        public DbSet<AB_Object_DB_Model> Models { get; set; } = null!;
        public DbSet<AB_Object_DB_Chat_Turn> ChatTurns { get; set; } = null!;

        public AB_Context_DB(DbContextOptions<AB_Context_DB> _options) : base(_options)
        {
        }

        protected override void OnModelCreating(ModelBuilder _builder)
        {
            // App.Model — PK = Id (convention). CredentialId nullable FK (FK 제약 강제 X — App.Credential row 없는 로컬 모델 정합).
            _builder.Entity<AB_Object_DB_App_Model>(_e =>
            {
                _e.HasKey(_x => _x.Id);
            });

            // App.Credential — PK = Id.
            _builder.Entity<AB_Object_DB_App_Credential>(_e =>
            {
                _e.HasKey(_x => _x.Id);
            });

            // Persona — PK = Id. is_active row 다중 = 부트 시점 첫 1 row 선택 (정책 결재 = 별도 그룹).
            _builder.Entity<AB_Object_DB_Persona>(_e =>
            {
                _e.HasKey(_x => _x.Id);
            });

            // Package — PK = Id. uuid 유일 index 매개 파일명 매칭 단일 row 보장.
            _builder.Entity<AB_Object_DB_Package>(_e =>
            {
                _e.HasKey(_x => _x.Id);
                _e.HasIndex(_x => _x.Uuid).IsUnique();
            });

            _builder.Entity<AB_Object_DB_Circuit>(_e =>
            {
                _e.HasKey(_x => _x.Id);
                _e.HasIndex(_x => _x.Uuid).IsUnique();
            });

            _builder.Entity<AB_Object_DB_Logic>(_e =>
            {
                _e.HasKey(_x => _x.Id);
                _e.HasIndex(_x => _x.Uuid).IsUnique();
            });

            _builder.Entity<AB_Object_DB_Model>(_e =>
            {
                _e.HasKey(_x => _x.Id);
                _e.HasIndex(_x => _x.Uuid).IsUnique();
            });

            // ChatTurn — PK = Id. PackageId 매개 매개 (CreatedAt 매개 매개 매개 sort) index 매개 매개 매개 매개 read 매개 매개 매개 매개.
            //   PackageId = 0 ("(선택 없음)" sentinel) 매개 매개 매개 매개 매개 매개 매개 매개 매개. FK 제약 X ([[feedback_fk_minimization_speed_customization]] 정합).
            _builder.Entity<AB_Object_DB_Chat_Turn>(_e =>
            {
                _e.HasKey(_x => _x.Id);
                _e.HasIndex(_x => _x.PackageId);
            });
        }
    }
}
