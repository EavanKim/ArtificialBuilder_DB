using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // 시계열 샤딩 DB 안 Turn 히스토리 entity row (= bucket).
    // 폴더 = `runtime/turn/{circuit_id}/` — circuit_id 는 폴더명 매개 식별 (본 entity 안 컬럼 X).
    // 폴더 안 파일 = `{turn_bucket}.db` — turn_bucket = turn_id / 100 (100 turn / 파일).
    // PK = TurnId — OnModelCreating 매개 등록 (AB_Context_DB_Turn_Shard).
    //
    // bucket 안 Result N 개 = AB_Object_DB_Node_Row 안 2 차원 배열 [result_seq] 매개 (Result 노드별 키 샤딩 DB).
    // refresh = bucket 안 result 추가 (selected_index 갱신).
    // storage-policy 2026-05-17 round 2 § "Turn 시계열 샤딩 DB 스키마" 1:1.
    // FK 최저화 룰 매개 PrevTurnId = plain column (FK 제약 X).
    public class AB_Object_DB_Turn_Row : AB_Object
    {
        // 턴 식별 (PK = bucket 식별 = 입력 차수).
        public long TurnId { get; set; }

        // 이전 턴 (linked list — 시계열). FK 제약 X — plain long.
        public long? PrevTurnId { get; set; }

        // 2 차원 배열 안 active result index (UI 표기 매개 — bucket 안 1 선택지만 노출, AI 채팅 ◀▶ UX).
        public int SelectedIndex { get; set; }

        // bucket 안 result 갯수 (배열 길이).
        public int ResultCount { get; set; }

        // Unix ms 생성 시각.
        public long CreatedAt { get; set; }

        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose() { }
    }
}
