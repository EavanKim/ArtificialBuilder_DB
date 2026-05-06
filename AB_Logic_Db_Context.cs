using Microsoft.EntityFrameworkCore;
using ArtificialBuilder.Models;

namespace ArtificialBuilder
{
    /// <summary>Logic DB 컨텍스트 (per-logic SQLite). 노드 정보만 — 사용 서킷 / Response UI 키 / sub-logic / 사용 history.</summary>
    public class AB_Logic_Db_Context : DbContext
    {
        /// <summary>Logic 메타 (단일 row).</summary>
        private DbSet<AB_Logic_Meta_Model> m_meta = null!;
        public DbSet<AB_Logic_Meta_Model> Meta
        {
            get { return m_meta; }
            set { m_meta = value; }
        }
        /// <summary>사용 서킷 list (이름 + 사용 키 + 서킷 내 system prompt + 끌어쓴 model key).</summary>
        private DbSet<AB_Logic_Used_Circuit_Model> m_usedCircuits = null!;
        public DbSet<AB_Logic_Used_Circuit_Model> UsedCircuits
        {
            get { return m_usedCircuits; }
            set { m_usedCircuits = value; }
        }
        /// <summary>사용 Response UI 키 list.</summary>
        private DbSet<AB_Logic_Used_Response_Ui_Model> m_usedResponseUi = null!;
        public DbSet<AB_Logic_Used_Response_Ui_Model> UsedResponseUi
        {
            get { return m_usedResponseUi; }
            set { m_usedResponseUi = value; }
        }
        /// <summary>sub-logic 참조 list (재귀).</summary>
        private DbSet<AB_Logic_Sub_Logic_Model> m_subLogics = null!;
        public DbSet<AB_Logic_Sub_Logic_Model> SubLogics
        {
            get { return m_subLogics; }
            set { m_subLogics = value; }
        }
        /// <summary>사용 history (turn = 로직 히스토리 동의어).</summary>
        private DbSet<AB_Logic_History_Turn_Model> m_historyTurns = null!;
        public DbSet<AB_Logic_History_Turn_Model> HistoryTurns
        {
            get { return m_historyTurns; }
            set { m_historyTurns = value; }
        }
        /// <summary>로직 내부 그래프 노드 (= AB_Logic_Node_* 인스턴스). v2 2026-05-06 후속 [[logic-internal-graph]].</summary>
        private DbSet<AB_Logic_Internal_Node_Model> m_internalNodes = null!;
        public DbSet<AB_Logic_Internal_Node_Model> InternalNodes
        {
            get { return m_internalNodes; }
            set { m_internalNodes = value; }
        }
        /// <summary>로직 내부 그래프 connection. v2 2026-05-06 후속 [[logic-internal-graph]].</summary>
        private DbSet<AB_Logic_Internal_Connection_Model> m_internalConnections = null!;
        public DbSet<AB_Logic_Internal_Connection_Model> InternalConnections
        {
            get { return m_internalConnections; }
            set { m_internalConnections = value; }
        }

        /// <summary>EF Core 옵션 주입 생성자.</summary>
        public AB_Logic_Db_Context(DbContextOptions<AB_Logic_Db_Context> _options)
            : base(_options)
        {
        }

        protected override void OnModelCreating(ModelBuilder _modelBuilder)
        {
            base.OnModelCreating(_modelBuilder);

            _modelBuilder.Entity<AB_Logic_Used_Circuit_Model>()
                .HasIndex(_m => _m.UseKey_)
                .IsUnique();

            _modelBuilder.Entity<AB_Logic_Used_Response_Ui_Model>()
                .HasIndex(_m => _m.UseKey_)
                .IsUnique();

            _modelBuilder.Entity<AB_Logic_Sub_Logic_Model>()
                .HasIndex(_m => _m.UseKey_)
                .IsUnique();

            _modelBuilder.Entity<AB_Logic_History_Turn_Model>()
                .HasIndex(_m => _m.TurnId_)
                .IsUnique();

            _modelBuilder.Entity<AB_Logic_Internal_Connection_Model>()
                .HasIndex(_m => new { _m.FromNodeId_, _m.ToNodeId_ });
        }
    }
}
