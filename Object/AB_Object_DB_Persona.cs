using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Persona DB 안 persona entity row. 최소집합 (id / name / is_active).
    //   - storage-policy 정본: per-persona = persona/{persona_uuid}.db 파일 매개 별 (현재 단일 DB skeleton — 별 파일 분기 = 별도 그룹).
    //   - PERSONA_DB_LOAD_ACTIVE 단일 쿼리 매개 is_active = true row 1 매개 부트 시점 active persona resolve.
    //   - App / Package 와 무관 — storage-policy 무관 도메인 룰 정합.
    // EF Core POCO — AB_Context_DB 매개 DbSet<AB_Object_DB_Persona>.
    public class AB_Object_DB_Persona : AB_Object
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose() { }
    }
}
