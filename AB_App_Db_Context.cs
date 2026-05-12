using Microsoft.EntityFrameworkCore;
using EDPFW.Models;
using ArtificialBuilder.Models;

namespace ArtificialBuilder
{
    /// <summary>전역 앱 DB 컨텍스트 (사용자/모델/UI 템플릿/파이프라인).</summary>
    public class AB_App_Db_Context : DbContext
    {
        /// <summary>사용자 테이블.</summary>
        private DbSet<AB_User_Model> m_users = null!;
        public DbSet<AB_User_Model> Users
        {
            get { return m_users; }
            set { m_users = value; }
        }
        /// <summary>LLM 모델 설정 테이블.</summary>
        private DbSet<AB_Model_Config_Model> m_modelConfigs = null!;
        public DbSet<AB_Model_Config_Model> ModelConfigs
        {
            get { return m_modelConfigs; }
            set { m_modelConfigs = value; }
        }
        /// <summary>전역 UI 템플릿 테이블.</summary>
        private DbSet<AB_Ui_Template_Model> m_uiTemplates = null!;
        public DbSet<AB_Ui_Template_Model> UiTemplates
        {
            get { return m_uiTemplates; }
            set { m_uiTemplates = value; }
        }
        /// <summary>파이프라인 템플릿 테이블.</summary>
        private DbSet<AB_Pipeline_Model> m_pipelines = null!;
        public DbSet<AB_Pipeline_Model> Pipelines
        {
            get { return m_pipelines; }
            set { m_pipelines = value; }
        }
        /// <summary>로컬 GGUF 모델 entity 테이블 (typed-id-edp-rebase chunk 4o).</summary>
        private DbSet<AB_Llama_Model> m_llamaModels = null!;
        public DbSet<AB_Llama_Model> LlamaModels
        {
            get { return m_llamaModels; }
            set { m_llamaModels = value; }
        }

        /// <summary>EF Core 옵션 주입 생성자.</summary>
        public AB_App_Db_Context(DbContextOptions<AB_App_Db_Context> _options)
            : base(_options)
        {
        }

        protected override void OnModelCreating(ModelBuilder _modelBuilder)
        {
            base.OnModelCreating(_modelBuilder);

            _modelBuilder.Entity<AB_Model_Config_Model>()
                .HasIndex(_m => _m.Name_)
                .IsUnique();

            _modelBuilder.Entity<AB_Llama_Model>()
                .HasIndex(_m => _m.FileName_)
                .IsUnique();
        }
    }
}
