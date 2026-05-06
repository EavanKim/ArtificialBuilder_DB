using Microsoft.EntityFrameworkCore;
using ArtificialBuilder.Models;
using ArtificialBuilder.Sharding;

namespace ArtificialBuilder
{
    /// <summary>페르소나 DB 컨텍스트 (페르소나별 채팅 세션/메시지).</summary>
    public class AB_Persona_Context : DbContext
    {
        /// <summary>페르소나 설정 (단일 행).</summary>
        private DbSet<AB_Persona_Settings_Model> m_settings = null!;
        public DbSet<AB_Persona_Settings_Model> Settings
        {
            get { return m_settings; }
            set { m_settings = value; }
        }
        /// <summary>채팅 세션 테이블.</summary>
        private DbSet<AB_Chat_Session_Model> m_chatSessions = null!;
        public DbSet<AB_Chat_Session_Model> ChatSessions
        {
            get { return m_chatSessions; }
            set { m_chatSessions = value; }
        }

        // Phase 4.4.d — session_data_pool DbSet 폐기. 4 계층 storage 의 ResourceStorage / NodeStorage 가 정본.
        // AB_MessageDataRefModel DbSet 은 Phase C 에서 폐기. 테이블 DROP 은 PersonaDb v2 에서.

        /// <summary>저장된 이미지 — 세션별 수동 저장 이미지.</summary>
        private DbSet<AB_Saved_Image_Model> m_savedImages = null!;
        public DbSet<AB_Saved_Image_Model> SavedImages
        {
            get { return m_savedImages; }
            set { m_savedImages = value; }
        }

        // ─── 4 계층 저장소 ([[storage-layers]]) — 신규. 기존 SessionDataPool / context_records / context_history 를 대체할 정본 ───

        /// <summary>Resource Storage — 페이로드 KV (4 계층 저장소의 1 번).</summary>
        private DbSet<AB_Resource_Storage_Model> m_resourceStorage = null!;
        public DbSet<AB_Resource_Storage_Model> ResourceStorage
        {
            get { return m_resourceStorage; }
            set { m_resourceStorage = value; }
        }

        /// <summary>Session Storage — turn 슬롯 1 직선 linked list (4 계층 저장소의 2 번).</summary>
        private DbSet<AB_Session_Storage_Model> m_sessionStorage = null!;
        public DbSet<AB_Session_Storage_Model> SessionStorage
        {
            get { return m_sessionStorage; }
            set { m_sessionStorage = value; }
        }

        /// <summary>Context Storage — 1 회 실행 단위 + 같은 turn 의 refresh 변종 (4 계층 저장소의 3 번).</summary>
        private DbSet<AB_Context_Storage_Model> m_contextStorage = null!;
        public DbSet<AB_Context_Storage_Model> ContextStorage
        {
            get { return m_contextStorage; }
            set { m_contextStorage = value; }
        }

        /// <summary>Node Storage — 1 노드 1 회 실행 + resource 키만 (4 계층 저장소의 4 번).</summary>
        private DbSet<AB_Logic_Storage_Model> m_nodeStorage = null!;
        public DbSet<AB_Logic_Storage_Model> NodeStorage
        {
            get { return m_nodeStorage; }
            set { m_nodeStorage = value; }
        }

        /// <summary>Pending Deletion — cascade 삭제 큐 (leaf-first time-budgeted sweep).</summary>
        private DbSet<AB_Pending_Deletion_Storage_Model> m_pendingDeletions = null!;
        public DbSet<AB_Pending_Deletion_Storage_Model> PendingDeletions
        {
            get { return m_pendingDeletions; }
            set { m_pendingDeletions = value; }
        }

        /// <summary>EF Core 옵션 주입 생성자.</summary>
        public AB_Persona_Context(DbContextOptions<AB_Persona_Context> _options)
            : base(_options)
        {
        }

        protected override void OnModelCreating(ModelBuilder _modelBuilder)
        {
            base.OnModelCreating(_modelBuilder);

            _modelBuilder.Entity<AB_Chat_Session_Model>()
                .HasIndex(_s => _s.PersonaId_);

            _modelBuilder.Entity<AB_Chat_Session_Model>()
                .HasIndex(_s => _s.CircuitName_);

            // Phase 4.4.d — session_data_pool 인덱스 폐기. 4 계층 storage 가 정본.
            // AB_MessageDataRefModel 모델 바인딩은 Phase C 에서 제거됨. 테이블 DROP 은 PersonaDb v2.

            // ─── 4 계층 저장소 인덱스 ([[storage-layers]]) ───

            _modelBuilder.Entity<AB_Session_Storage_Model>()
                .HasIndex(_s => _s.SessionId_);

            _modelBuilder.Entity<AB_Context_Storage_Model>()
                .HasIndex(_c => _c.SessionId_);
            _modelBuilder.Entity<AB_Context_Storage_Model>()
                .HasIndex(_c => _c.TurnId_);

            _modelBuilder.Entity<AB_Logic_Storage_Model>()
                .HasIndex(_n => _n.ContextId_);
            _modelBuilder.Entity<AB_Logic_Storage_Model>()
                .HasIndex(_n => new { _n.ContextId_, _n.NodeId_, _n.EmissionOrder_ })
                .IsUnique();
        }
    }
}
