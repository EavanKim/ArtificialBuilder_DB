using Microsoft.EntityFrameworkCore;
using ArtificialBuilder.Models;

// TODO[db-three-way-split]: sub 3 — Circuit DB 정합 (자원 보존 + Response UI 4 종 분리 + Used_Sub_Circuit / Hosted_Logic 신설). 폐기 DbSet: ResponseWindows / WindowComponents / UiTemplates (→ Response UI DB sub 2). 보존: Settings/Lore/Asset/Character/Location/Pattern/Data. plan: docs/plans/doing/db-three-way-split/3-circuit-db-simplify.md
namespace ArtificialBuilder
{
    /// <summary>Circuit DB 컨텍스트 (Circuit별 설정/로어/캐릭터/장소/UI 등).</summary>
    public class AB_Circuit_Db_Context : DbContext
    {
        /// <summary>Circuit 설정 (단일 행).</summary>
        private DbSet<AB_Circuit_Settings_Model> m_settings = null!;
        public DbSet<AB_Circuit_Settings_Model> Settings
        {
            get { return m_settings; }
            set { m_settings = value; }
        }
        /// <summary>로어북 엔트리.</summary>
        private DbSet<AB_Lore_Entry_Model> m_loreEntries = null!;
        public DbSet<AB_Lore_Entry_Model> LoreEntries
        {
            get { return m_loreEntries; }
            set { m_loreEntries = value; }
        }
        /// <summary>Circuit 에셋 (이미지/사운드/폰트).</summary>
        private DbSet<AB_Circuit_Asset_Model> m_assets = null!;
        public DbSet<AB_Circuit_Asset_Model> Assets
        {
            get { return m_assets; }
            set { m_assets = value; }
        }
        /// <summary>캐릭터 정의.</summary>
        private DbSet<AB_Character_Model> m_characters = null!;
        public DbSet<AB_Character_Model> Characters
        {
            get { return m_characters; }
            set { m_characters = value; }
        }
        /// <summary>장소 정의.</summary>
        private DbSet<AB_Location_Model> m_locations = null!;
        public DbSet<AB_Location_Model> Locations
        {
            get { return m_locations; }
            set { m_locations = value; }
        }
        /// <summary>캐릭터 간 관계.</summary>
        private DbSet<AB_Character_Relationship_Model> m_characterRelationships = null!;
        public DbSet<AB_Character_Relationship_Model> CharacterRelationships
        {
            get { return m_characterRelationships; }
            set { m_characterRelationships = value; }
        }
        /// <summary>장소 간 연결.</summary>
        private DbSet<AB_Location_Connection_Model> m_locationConnections = null!;
        public DbSet<AB_Location_Connection_Model> LocationConnections
        {
            get { return m_locationConnections; }
            set { m_locationConnections = value; }
        }
        /// <summary>Circuit UI 템플릿.</summary>
        private DbSet<AB_Circuit_Ui_Template_Model> m_uiTemplates = null!;
        public DbSet<AB_Circuit_Ui_Template_Model> UiTemplates
        {
            get { return m_uiTemplates; }
            set { m_uiTemplates = value; }
        }
        /// <summary>패턴 추출 설정.</summary>
        private DbSet<AB_Pattern_Config_Model> m_patternConfigs = null!;
        public DbSet<AB_Pattern_Config_Model> PatternConfigs
        {
            get { return m_patternConfigs; }
            set { m_patternConfigs = value; }
        }
        /// <summary>관계 색상 매핑.</summary>
        private DbSet<AB_Relation_Color_Model> m_relationColors = null!;
        public DbSet<AB_Relation_Color_Model> RelationColors
        {
            get { return m_relationColors; }
            set { m_relationColors = value; }
        }
        /// <summary>캐릭터 동적 데이터 카테고리.</summary>
        private DbSet<AB_Data_Category_Model> m_dataCategories = null!;
        public DbSet<AB_Data_Category_Model> DataCategories
        {
            get { return m_dataCategories; }
            set { m_dataCategories = value; }
        }
        /// <summary>캐릭터 동적 데이터.</summary>
        private DbSet<AB_Character_Data_Model> m_characterData = null!;
        public DbSet<AB_Character_Data_Model> CharacterData
        {
            get { return m_characterData; }
            set { m_characterData = value; }
        }
        /// <summary>응답 윈도우 정의.</summary>
        private DbSet<AB_Response_Window_Model> m_responseWindows = null!;
        public DbSet<AB_Response_Window_Model> ResponseWindows
        {
            get { return m_responseWindows; }
            set { m_responseWindows = value; }
        }
        /// <summary>윈도우에 부착된 컴포넌트 (Frame/Layout/Depth + 성격).</summary>
        private DbSet<AB_Window_Component_Model> m_windowComponents = null!;
        public DbSet<AB_Window_Component_Model> WindowComponents
        {
            get { return m_windowComponents; }
            set { m_windowComponents = value; }
        }

        /// <summary>EF Core 옵션 주입 생성자.</summary>
        public AB_Circuit_Db_Context(DbContextOptions<AB_Circuit_Db_Context> _options)
            : base(_options)
        {
        }

        // 레거시 response_windows.display_mode shadow property + SaveChanges 주입 오버라이드는 제거됨 — EF 모델 정상화로 처리.
    }
}
