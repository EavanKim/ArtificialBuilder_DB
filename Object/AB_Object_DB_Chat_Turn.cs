using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Chat history turn entity row. 1 row = 1 user/assistant message (linear sequence per package).
    //   PackageId = AB_Object_DB_Package.Id 매개 매개 매개 (0 = "(선택 없음)" sentinel 매개 매개 매개 매개 매개).
    //   Role = "user" | "assistant" (system 매개 매개 매개 매개 매개 매개 매개 — system prompt 매개 매개 매개 Logic/Circuit Content 매개 매개).
    //   Content = 매개 매개 매개 text.
    //   CreatedAt = Unix ms — INFO_GET_BY_PACKAGE 매개 매개 sort key.
    // 사용자 정본 2026-05-18: chat history 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개.
    public class AB_Object_DB_Chat_Turn : AB_Object
    {
        public long Id { get; set; }
        public long PackageId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public long CreatedAt { get; set; }

        // 응답 매개 매개 매개 token (assistant role 매개 매개). user 매개 = 0.
        //   ModelProvider / ModelId 매개 매개 cost 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개 매개.
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public string ModelProvider { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;

        public override void ReceiveMessage(int _header_id, long _data_key) { }
        public override void Dispose() { }
    }
}
