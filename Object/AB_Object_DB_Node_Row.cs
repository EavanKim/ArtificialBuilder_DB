using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Result 노드별 키 샤딩 DB 안 Result 페이로드 entity row.
    // 폴더 = `runtime/result/{circuit_id}/{node_number}/` — circuit_id / node_number 는 폴더명 매개 식별 (본 entity 안 컬럼 X).
    // 폴더 안 파일 = `{data_key_range_100}.db` — data_key range 매개 분할 (100 result / 파일).
    // PK = DataKey 단일 ([[feedback_key_single]] 정합) — 조인 동작 매개 단일 키.
    //
    // storage-policy 2026-05-17 round 2 § "Result 노드별 키 샤딩 DB 스키마" 1:1.
    // FK 최저화 룰 매개 TurnId = plain column (FK 제약 X). 페이로드 = inline blob 단일 (외부 파일 X).
    // node_numbers = 폴더 = 1 노드 매개 식별 + 본 컬럼 = 추가 ref / cross-node 조인 / 디버깅 매개.
    // EF Core POCO — AB_Context_DB_Node_Shard 매개 DbSet<AB_Object_DB_Node_Row>.
    public class AB_Object_DB_Node_Row : AB_Object
    {
        // 단일 데이터 키 (PK).
        public long DataKey { get; set; }

        // 턴 식별 (참조 컬럼 — FK 제약 X, plain long).
        public long TurnId { get; set; }

        // bucket 2 차원 배열 안 row index (0 = 1 차 / 1~ = refresh).
        public int ResultSeq { get; set; }

        // 사용 노드 번호 묶음 — packed ulong[] (서킷 내부 노드, 0 = sentinel / 1~ = 정상). 폴더 노드 외 cross-node ref.
        public byte[]? NodeNumbers { get; set; }

        // 페이로드 본체 (text / image / audio / video / embedding) — inline blob 단일.
        public byte[]? Payload { get; set; }

        // 생성 시각 (Unix ms).
        public long CreatedAt { get; set; }

        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose() { }
    }
}
