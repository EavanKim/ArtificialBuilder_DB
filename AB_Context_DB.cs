using Microsoft.EntityFrameworkCore;

namespace ArtificialBuilder.DB
{
    // AB_Manager_DB 단일 내부 DbContext. 외부 노출 X — internal sealed (AB_Manager_DB 만 instantiate).
    // 모든 entity DbSet 망라 (5 도메인 App / Persona / Circuit / Logic / Response_UI) — DbSet 본체 = 도메인 entity 결재 후 (별도 그룹).
    //   MS Learn 정본: "DbContext instance represents a session with the database... Unit Of Work + Repository patterns".
    //   단일 인스턴스 매개 모든 entity 망라 — 도메인별 / sharding 분리 X (사용자 정본 2026-05-15).
    // 본 skeleton = DbSet 미정의 — 도메인 entity 신설 시 본 파일 안 DbSet<T> 프로퍼티 추가.
    internal sealed class AB_Context_DB : DbContext
    {
        public AB_Context_DB(DbContextOptions<AB_Context_DB> _options) : base(_options)
        {
        }
    }
}
