using Microsoft.EntityFrameworkCore;
using ArtificialBuilder.Models;

namespace ArtificialBuilder
{
    /// <summary>Response UI DB 컨텍스트 (per-response-ui SQLite). 화면 구성 (Window / Component / Layer / Template).</summary>
    public class AB_Response_Ui_Db_Context : DbContext
    {
        /// <summary>Response UI 메타 (단일 row).</summary>
        private DbSet<AB_Response_Ui_Meta_Model> m_meta = null!;
        public DbSet<AB_Response_Ui_Meta_Model> Meta
        {
            get { return m_meta; }
            set { m_meta = value; }
        }
        /// <summary>Response Window 정의.</summary>
        private DbSet<AB_Response_Ui_Window_Model> m_windows = null!;
        public DbSet<AB_Response_Ui_Window_Model> Windows
        {
            get { return m_windows; }
            set { m_windows = value; }
        }
        /// <summary>Window 안 Component 정의 (Frame / Layout / Depth + 출력 레이어 본체).</summary>
        private DbSet<AB_Response_Ui_Component_Model> m_components = null!;
        public DbSet<AB_Response_Ui_Component_Model> Components
        {
            get { return m_components; }
            set { m_components = value; }
        }
        /// <summary>Layer 정의 (UUID + 사용자 표시 이름).</summary>
        private DbSet<AB_Response_Ui_Layer_Model> m_layers = null!;
        public DbSet<AB_Response_Ui_Layer_Model> Layers
        {
            get { return m_layers; }
            set { m_layers = value; }
        }
        /// <summary>XML 템플릿.</summary>
        private DbSet<AB_Response_Ui_Template_Model> m_templates = null!;
        public DbSet<AB_Response_Ui_Template_Model> Templates
        {
            get { return m_templates; }
            set { m_templates = value; }
        }

        /// <summary>EF Core 옵션 주입 생성자.</summary>
        public AB_Response_Ui_Db_Context(DbContextOptions<AB_Response_Ui_Db_Context> _options)
            : base(_options)
        {
        }
    }
}
