using Microsoft.EntityFrameworkCore;
using ArtificialBuilder.Models;

namespace ArtificialBuilder
{
    /// <summary>파이프라인 디버그 DB 컨텍스트. 디버그 엔트리 전용 테이블.</summary>
    public class AB_Pipeline_Debug_Db_Context : DbContext
    {
        private DbSet<AB_Pipeline_Debug_Entry_Model> m_entries = null!;
        public DbSet<AB_Pipeline_Debug_Entry_Model> Entries
        {
            get { return m_entries; }
            set { m_entries = value; }
        }


        public AB_Pipeline_Debug_Db_Context(DbContextOptions<AB_Pipeline_Debug_Db_Context> _options)
            : base(_options)
        {
        }

        protected override void OnModelCreating(ModelBuilder _modelBuilder)
        {
            base.OnModelCreating(_modelBuilder);

            _modelBuilder.Entity<AB_Pipeline_Debug_Entry_Model>()
                .HasIndex(_e => _e.SessionId_)
                .HasDatabaseName("ix_pipeline_debug_session");

            _modelBuilder.Entity<AB_Pipeline_Debug_Entry_Model>()
                .HasIndex(_e => _e.TimestampUtc_)
                .HasDatabaseName("ix_pipeline_debug_ts");

            _modelBuilder.Entity<AB_Pipeline_Debug_Entry_Model>()
                .HasIndex(_e => new { _e.SessionId_, _e.EntryType_ })
                .HasDatabaseName("ix_pipeline_debug_session_type");

        }
    }
}
