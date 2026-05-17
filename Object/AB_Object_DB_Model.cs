using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Model DB entity row.
    // ModelType = chat / image / audio / video / embedding (UI 매개 LLM/VLM/ALM/VGM/EMB 매핑).
    // Provider = anthropic / openai / grok / openrouter / local / gemini — AB_Object_AI_<X> 매개 콘크리트 매핑.
    // ProviderModelId = provider 매개 model 식별자 (예: "claude-3-5-sonnet-20241022" / "gpt-4o" / "all-minilm").
    // ApiKey = provider 매개 인증 키. Local (Ollama loopback) 매개 빈 string OK.
    public class AB_Object_DB_Model : AB_Object
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ModelType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ProviderModelId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;

        public override void ReceiveMessage(int _header_id, long _data_key) { }
        public override void Dispose() { }
    }
}
