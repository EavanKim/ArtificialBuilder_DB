using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtificialBuilder.Models
{
    /// <summary>Circuit 단일 설정 (시스템 프롬프트/모델/외형/파이프라인 등). singleton (PK fixed = 1). (example-mental-restructure Phase B Sub 4 Circuit 8/8) — string PK → long PK.</summary>
    [Table("circuit_settings")]
    public class AB_Circuit_Settings_Model
    {
        private long m_id_ = 1;
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        private string? m_systemPrompt_;
        [Column("system_prompt")]
        public string? SystemPrompt_
        {
            get { return m_systemPrompt_; }
            set { m_systemPrompt_ = value; }
        }

        private string? m_requestBody_;
        [Column("extra_config")]
        public string? RequestBody_
        {
            get { return m_requestBody_; }
            set { m_requestBody_ = value; }
        }

        private int m_maxInputTokens_ = 0;
        [Column("max_input_tokens")]
        public int MaxInputTokens_
        {
            get { return m_maxInputTokens_; }
            set { m_maxInputTokens_ = value; }
        }

        private int m_maxOutputTokens_ = 0;
        [Column("max_output_tokens")]
        public int MaxOutputTokens_
        {
            get { return m_maxOutputTokens_; }
            set { m_maxOutputTokens_ = value; }
        }

        private string m_displayMode_ = "normal";
        [Column("display_mode")]
        public string DisplayMode_
        {
            get { return m_displayMode_; }
            set { m_displayMode_ = value; }
        }

        private string? m_xmlTemplate_;
        [Column("xml_template")]
        public string? XmlTemplate_
        {
            get { return m_xmlTemplate_; }
            set { m_xmlTemplate_ = value; }
        }

        // show_preview 컬럼은 v2 마이그레이션에서 DROP. 프로퍼티 제거 — 옛 UI 체크박스 이미 소멸.

        private string? m_modelId_;
        [Column("model_id")]
        public string? ModelId_
        {
            get { return m_modelId_; }
            set { m_modelId_ = value; }
        }

        private byte[]? m_iconData_;
        [Column("icon_data")]
        public byte[]? IconData_
        {
            get { return m_iconData_; }
            set { m_iconData_ = value; }
        }

        private bool m_autoScroll_ = true;
        [Column("auto_scroll")]
        public bool AutoScroll_
        {
            get { return m_autoScroll_; }
            set { m_autoScroll_ = value; }
        }

        private bool m_embeddingEnabled_ = false;
        [Column("embedding_enabled")]
        public bool EmbeddingEnabled_
        {
            get { return m_embeddingEnabled_; }
            set { m_embeddingEnabled_ = value; }
        }

        private string? m_embeddingModel_;
        [Column("embedding_model")]
        public string? EmbeddingModel_
        {
            get { return m_embeddingModel_; }
            set { m_embeddingModel_ = value; }
        }

        private string? m_embeddingProvider_;
        [Column("embedding_provider")]
        public string? EmbeddingProvider_
        {
            get { return m_embeddingProvider_; }
            set { m_embeddingProvider_ = value; }
        }

        private int m_loreTopK_ = 5;
        [Column("lore_top_k")]
        public int LoreTopK_
        {
            get { return m_loreTopK_; }
            set { m_loreTopK_ = value; }
        }

        private int m_memoryTopK_ = 10;
        [Column("memory_top_k")]
        public int MemoryTopK_
        {
            get { return m_memoryTopK_; }
            set { m_memoryTopK_ = value; }
        }

        private bool m_memoryEnabled_ = false;
        [Column("memory_enabled")]
        public bool MemoryEnabled_
        {
            get { return m_memoryEnabled_; }
            set { m_memoryEnabled_ = value; }
        }

        private DateTime m_updatedAt_ = DateTime.UtcNow;
        [Column("updated_at")]
        public DateTime UpdatedAt_
        {
            get { return m_updatedAt_; }
            set { m_updatedAt_ = value; }
        }

        // --- Circuit 타입 ---

        private string m_circuitType_ = "normal";
        [Column("circuit_type")]
        public string CircuitType_
        {
            get { return m_circuitType_; }
            set { m_circuitType_ = value; }
        }

        // --- 캐릭터/시뮬레이션 전용 필드 ---

        private string? m_charName_;
        [Column("char_name")]
        public string? CharName_
        {
            get { return m_charName_; }
            set { m_charName_ = value; }
        }

        private string? m_charPersonality_;
        [Column("char_personality")]
        public string? CharPersonality_
        {
            get { return m_charPersonality_; }
            set { m_charPersonality_ = value; }
        }

        private string? m_charGreeting_;
        [Column("char_greeting")]
        public string? CharGreeting_
        {
            get { return m_charGreeting_; }
            set { m_charGreeting_ = value; }
        }

        private string? m_charBackstory_;
        [Column("char_backstory")]
        public string? CharBackstory_
        {
            get { return m_charBackstory_; }
            set { m_charBackstory_ = value; }
        }

        private string? m_charCreatorNotes_;
        [Column("char_creator_notes")]
        public string? CharCreatorNotes_
        {
            get { return m_charCreatorNotes_; }
            set { m_charCreatorNotes_ = value; }
        }

        private string m_charSource_ = "user";
        [Column("char_source")]
        public string CharSource_
        {
            get { return m_charSource_; }
            set { m_charSource_ = value; }
        }

        // --- 나레이션 모드 ---

        private bool m_narrationEnabled_ = true;
        [Column("narration_enabled")]
        public bool NarrationEnabled_
        {
            get { return m_narrationEnabled_; }
            set { m_narrationEnabled_ = value; }
        }

        // --- 외형 ---

        private string? m_iconColor_;
        [Column("icon_color")]
        public string? IconColor_
        {
            get { return m_iconColor_; }
            set { m_iconColor_ = value; }
        }

        private string? m_iconAsset_;
        [Column("icon_asset")]
        public string? IconAsset_
        {
            get { return m_iconAsset_; }
            set { m_iconAsset_ = value; }
        }

        private string? m_nameColor_;
        [Column("name_color")]
        public string? NameColor_
        {
            get { return m_nameColor_; }
            set { m_nameColor_ = value; }
        }

        private string? m_nameFontAsset_;
        [Column("name_font_asset")]
        public string? NameFontAsset_
        {
            get { return m_nameFontAsset_; }
            set { m_nameFontAsset_ = value; }
        }

        private string? m_bgColor_;
        [Column("bg_color")]
        public string? BgColor_
        {
            get { return m_bgColor_; }
            set { m_bgColor_ = value; }
        }

        // --- 캐릭터 동적 데이터 토글 ---

        private bool m_cDataPatternEnabled_ = false;
        [Column("cdata_pattern_enabled")]
        public bool CDataPatternEnabled_
        {
            get { return m_cDataPatternEnabled_; }
            set { m_cDataPatternEnabled_ = value; }
        }

        private bool m_cDataCodeblockEnabled_ = false;
        [Column("cdata_codeblock_enabled")]
        public bool CDataCodeblockEnabled_
        {
            get { return m_cDataCodeblockEnabled_; }
            set { m_cDataCodeblockEnabled_ = value; }
        }

        private bool m_cDataManualEnabled_ = false;
        [Column("cdata_manual_enabled")]
        public bool CDataManualEnabled_
        {
            get { return m_cDataManualEnabled_; }
            set { m_cDataManualEnabled_ = value; }
        }

        // --- 비용 제한 ---

        private bool m_budgetEnabled_ = false;
        [Column("budget_enabled")]
        public bool BudgetEnabled_
        {
            get { return m_budgetEnabled_; }
            set { m_budgetEnabled_ = value; }
        }

        private decimal m_budgetPerSession_ = 0;
        [Column("budget_per_session")]
        public decimal BudgetPerSession_
        {
            get { return m_budgetPerSession_; }
            set { m_budgetPerSession_ = value; }
        }

        private string m_budgetAction_ = "warn";
        [Column("budget_action")]
        public string BudgetAction_
        {
            get { return m_budgetAction_; }
            set { m_budgetAction_ = value; }
        }

        // --- 파이프라인 ---

        /// <summary>(현재 미사용 — 1:1 Circuit 임베디드 그래프로 전환) 레거시 파이프라인 ID. NodeLayout_ 인라인 그래프만 사용.</summary>
        private string? m_pipelineId_;
        [Column("pipeline_id")]
        public string? PipelineId_
        {
            get { return m_pipelineId_; }
            set { m_pipelineId_ = value; }
        }

        // --- 로직 에디터 레이아웃 (인라인, 하위 호환) ---

        private string? m_nodeLayout_;
        [Column("node_layout")]
        public string? NodeLayout_
        {
            get { return m_nodeLayout_; }
            set { m_nodeLayout_ = value; }
        }

        // --- 윈도우 배치 레이아웃 ---

        private string? m_windowLayout_;
        [Column("window_layout")]
        public string? WindowLayout_
        {
            get { return m_windowLayout_; }
            set { m_windowLayout_ = value; }
        }

        // --- 홈 메시지 ---

        private string? m_homeMessage_;
        [Column("home_message")]
        public string? HomeMessage_
        {
            get { return m_homeMessage_; }
            set { m_homeMessage_ = value; }
        }

        private bool m_homeReadonly_ = false;
        [Column("home_readonly")]
        public bool HomeReadonly_
        {
            get { return m_homeReadonly_; }
            set { m_homeReadonly_ = value; }
        }

        // --- 세션 그리팅 ---

        private string? m_sessionGreeting_;
        [Column("session_greeting")]
        public string? SessionGreeting_
        {
            get { return m_sessionGreeting_; }
            set { m_sessionGreeting_ = value; }
        }

        private string m_sessionGreetingRole_ = "assistant";
        [Column("session_greeting_role")]
        public string SessionGreetingRole_
        {
            get { return m_sessionGreetingRole_; }
            set { m_sessionGreetingRole_ = value; }
        }

        // --- 도입 메시지 (Circuit 홈/새 채팅 미리보기) ---

        private string? m_introMessage_;
        [Column("intro_message")]
        public string? IntroMessage_
        {
            get { return m_introMessage_; }
            set { m_introMessage_ = value; }
        }

        private string? m_introImageAsset_;
        [Column("intro_image_asset")]
        public string? IntroImageAsset_
        {
            get { return m_introImageAsset_; }
            set { m_introImageAsset_ = value; }
        }

        // --- Circuit 테마 ---

        private string? m_theme_;
        [Column("theme")]
        public string? Theme_
        {
            get { return m_theme_; }
            set { m_theme_ = value; }
        }

        // --- LLM 초기 프롬프트 ---

        private string? m_llmInitPrompt_;
        [Column("llm_init_prompt")]
        public string? LlmInitPrompt_
        {
            get { return m_llmInitPrompt_; }
            set { m_llmInitPrompt_ = value; }
        }

        // --- 파이프라인 샤드 기본값 ---

        /// <summary>이 Circuit으로 생성되는 새 세션의 기본 TurnShardSize (턴 블록 크기).</summary>
        private int m_defaultTurnShardSize_ = 50;
        [Column("default_turn_shard_size")]
        public int DefaultTurnShardSize_
        {
            get { return m_defaultTurnShardSize_; }
            set { m_defaultTurnShardSize_ = value; }
        }

        // --- Chat view 바인딩 ---

        /// <summary>
        /// 기본 채팅 창으로 쓰일 AB_Response_Ui_Window_Model.Id_.
        /// [[project_display_binding_principle]] — chat view 필터 기준 window ID.
        /// Circuit 생성 시 기본 "메시지 영역" 윈도우 ID 자동 세팅.
        /// </summary>
        private string? m_primaryChatWindowId_;
        [Column("primary_chat_window_id")]
        public string? PrimaryChatWindowId_
        {
            get { return m_primaryChatWindowId_; }
            set { m_primaryChatWindowId_ = value; }
        }
    }
}
