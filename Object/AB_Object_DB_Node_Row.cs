using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // 키 샤딩 DB 안 Node 동작 페이로드 entity row.
    // 폴더 = `runtime/node/{node_id}/` — node_id 는 폴더명 매개 식별 (본 entity 안 컬럼 X).
    // 폴더 안 파일 = `{shard_key}.db` — shard_key 산정 정책 D2 round 2.
    // PK = (turn_id, logic_invocation_id) — OnModelCreating 매개 등록 (AB_Context_DB_Node_Shard).
    //
    // storage-policy 2026-05-17 § "키 샤딩 DB 스키마" 1:1.
    // EF Core POCO — AB_Context_DB_Node_Shard 매개 DbSet<AB_Object_DB_Node_Row>.
    public class AB_Object_DB_Node_Row : AB_Object
    {
        // 턴 식별 (FK → 시계열 샤딩 DB).
        public long TurnId { get; set; }

        // 로직 1 회 호출 id (재귀 / sub-logic 식별).
        public long LogicInvocationId { get; set; }

        // 페이로드 종류 enum int — "text" / "image" / "audio" / "video" / "embedding".
        public int PayloadKind { get; set; }

        // 작은 페이로드 inline.
        public byte[]? PayloadInline { get; set; }

        // 큰 페이로드 외부 파일 경로.
        public string? PayloadPath { get; set; }

        // 생성 시각 (Unix ms).
        public long CreatedAt { get; set; }

        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose() { }
    }
}
