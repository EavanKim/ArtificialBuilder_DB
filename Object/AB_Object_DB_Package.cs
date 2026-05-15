using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Package DB 안 package entity row. 최소집합 (id / uuid / name / version).
    //   - storage-policy 정본: per-package = package/{package_uuid}.db 파일 매개 별. uuid = 파일명 매칭 키.
    //   - Persona / App 와 무관 — storage-policy 무관 도메인 룰 정합.
    //   - 본 entity = info 메타만. dependencies / 포함 circuit / logic uuid 등 = 별도 그룹 매개 확장.
    // EF Core POCO — AB_Context_DB 매개 DbSet<AB_Object_DB_Package>.
    public class AB_Object_DB_Package : AB_Object
    {
        public long Id { get; set; }

        // 파일명 매칭 키 (`package/{uuid}.db`).
        public string Uuid { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        // semver 문자열.
        public string Version { get; set; } = string.Empty;

        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose() { }
    }
}
