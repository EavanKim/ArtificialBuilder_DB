using Microsoft.EntityFrameworkCore;
using ArtificialBuilder.Models;

namespace ArtificialBuilder
{
    /// <summary>Circuit DB 컨텍스트 ([[app-logic-separation]] — 사용자 완성품 / 노드 그래프 + 자원 + 다른 서킷 invoke 참조 + 호스팅 Logic 인스턴스 list). Response UI 4 종은 Response UI DB 로 분리.</summary>
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
        /// <summary>서킷 안 노드(로직) 가 invoke 한 다른 서킷 list (호출 노드 id / 대상 서킷 id / 사용 키).</summary>
        private DbSet<AB_Circuit_Used_Sub_Circuit_Model> m_usedSubCircuits = null!;
        public DbSet<AB_Circuit_Used_Sub_Circuit_Model> UsedSubCircuits
        {
            get { return m_usedSubCircuits; }
            set { m_usedSubCircuits = value; }
        }
        /// <summary>서킷이 호스팅하는 Logic 인스턴스 list (Logic UUID 참조 + 그래프 위치).</summary>
        private DbSet<AB_Circuit_Hosted_Logic_Model> m_hostedLogics = null!;
        public DbSet<AB_Circuit_Hosted_Logic_Model> HostedLogics
        {
            get { return m_hostedLogics; }
            set { m_hostedLogics = value; }
        }

        /// <summary>EF Core 옵션 주입 생성자.</summary>
        public AB_Circuit_Db_Context(DbContextOptions<AB_Circuit_Db_Context> _options)
            : base(_options)
        {
        }
    }
}
