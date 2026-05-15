using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // App DB 안 AI 모델 카탈로그 entity row. 사용자 설정 UI 매개 등록한 모델 메타.
    //   - 글로벌 1 (사용자 디바이스 단위). Persona / Package 와 무관 — storage-policy 무관 도메인 룰 정합.
    //   - Persona / Logic inference 시점 본 row.Id ref 매개 provider / path resolve.
    // EF Core POCO — AB_Manager_DB 내부 AB_Context_DB 매개 DbSet<AB_Object_DB_App_Model>.
    // ReceiveMessage / Dispose = AB_Object abstract 충족 매개 빈 override.
    public class AB_Object_DB_App_Model : AB_Object
    {
        public long Id { get; set; }

        // 사용자 라벨 (UI 표시용).
        public string Name { get; set; } = string.Empty;

        // 'llama' / 'openai' / 'anthropic' 등.
        public string Provider { get; set; } = string.Empty;

        // 로컬 gguf 경로 또는 endpoint URL.
        public string Path { get; set; } = string.Empty;

        // API key / 비밀 자격증명 ref. nullable — 로컬 모델 매개 credential X.
        public long? CredentialId { get; set; }

        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose() { }
    }
}
