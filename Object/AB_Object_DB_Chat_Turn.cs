using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Chat history turn entity row. 1 row = 1 user/assistant message (linear sequence per package).
    //   PackageId = AB_Object_DB_Package.Id 외래 ref (0 = "(선택 없음)" sentinel — 패키지 미선택 history).
    //   Role = "user" | "assistant" (system 은 별도 저장 X — system prompt 는 Logic/Circuit Content 에서 빌드).
    //   Content = 메시지 text.
    //   CreatedAt = Unix ms — INFO_GET_BY_PACKAGE 시 sort key.
    // 사용자 정본 2026-05-18: chat history 를 앱 재시작 후에도 유지.
    public class AB_Object_DB_Chat_Turn : AB_Object
    {
        public long Id { get; set; }
        public long PackageId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public long CreatedAt { get; set; }

        // 응답 1 회 token (assistant role 만 채움). user row = 0.
        //   ModelProvider / ModelId 는 누적 cost 계산용 — turn 별로 다른 모델 사용 가능성 대비.
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public string ModelProvider { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;

        public override void ReceiveMessage(int _header_id, long _data_key) { }
        public override void Dispose() { }
    }
}
