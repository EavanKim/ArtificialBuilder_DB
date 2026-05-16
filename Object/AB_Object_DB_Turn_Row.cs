using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // 시계열 샤딩 DB 안 Turn 히스토리 entity row.
    // 폴더 = `runtime/turn/{circuit_id}/` — circuit_id 는 폴더명 매개 식별 (본 entity 안 컬럼 X).
    // 폴더 안 파일 = `{turn_bucket}.db` — turn_bucket 산정 정책 D1 round 2.
    // PK = turn_id — OnModelCreating 매개 등록 (AB_Context_DB_Turn_Shard).
    //
    // refresh = 같은 turn 안 갈래 = BranchRootTurnId 매개 (산술 max+1 X — feedback_refresh_as_branch 정합).
    // storage-policy 2026-05-17 § "시계열 샤딩 DB 스키마" 1:1.
    // EF Core POCO — AB_Context_DB_Turn_Shard 매개 DbSet<AB_Object_DB_Turn_Row>.
    public class AB_Object_DB_Turn_Row : AB_Object
    {
        // 턴 식별 (PK).
        public long TurnId { get; set; }

        // 이전 턴 (linked list — 시계열).
        public long? PrevTurnId { get; set; }

        // refresh / 갈래 root.
        public long? BranchRootTurnId { get; set; }

        // 본 턴 매개 호출된 logic id.
        public long LogicIdInvoked { get; set; }

        // Unix ms 시작 시각.
        public long StartedAt { get; set; }

        // Unix ms 종료 시각 (in-progress = null).
        public long? EndedAt { get; set; }

        // enum int — None / Running / Done / Error / End.
        public int Status { get; set; }

        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose() { }
    }
}
