using Microsoft.EntityFrameworkCore;

namespace ArtificialBuilder
{
    /// <summary>벡터 저장소 전용 DB 컨텍스트 (별도 .vec 파일). 현재 미사용 — vec 테이블은 AB_Vec_Store가 raw SQL로 직접 관리.</summary>
    public class AB_Vec_Db_Context : DbContext
    {
        /// <summary>EF Core 옵션 주입 생성자.</summary>
        public AB_Vec_Db_Context(DbContextOptions<AB_Vec_Db_Context> _options)
            : base(_options)
        {
        }
    }
}
