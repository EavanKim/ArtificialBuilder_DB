using ArtificialBuilder.Common.Base;
using EDPFW;

namespace ArtificialBuilder.DB
{
    // 모듈 최상위 매니저. AB_Manager (root 4) 후손 — 별도 생존주기 / 풀 X / Loop 등록 X. DI Container singleton 매개 lookup.
    // EDP_Db_Engine 단일 보유. 프로그램 단위 lifetime. DB = file SQLite 단일 — 도메인별 / 멀티 머신 / sharding 분리 X (사용자 정본 2026-05-15).
    // DbContext (Microsoft.EntityFrameworkCore v10.0.0) = 본 매니저 내부 전용. 외부 노출 X. 단일 인스턴스 매개 모든 entity DbSet 망라.
    //   MS Learn 정본: "DbContext instance represents a session with the database... Unit Of Work + Repository patterns".
    //   threading: "EF Core does not support multiple parallel operations being run on the same DbContext instance" — entry.Lock (SemaphoreSlim) 매개 직렬화 (EDP_Db_Engine 안 정합).
    // CRUD 작업 진입 = 외부 caller 매개 본 매니저 내부 큐 enqueue → 본 매니저 매개 큐 drain 매개 DbContext 매개 트랜잭셔널 직렬 처리.
    //   (현 group = skeleton 만. 큐 envelope 형식 / Command_Type / 실 entity DbSet = 별도 그룹).
    public class AB_Manager_DB : AB_Manager
    {
        private readonly EDP_Db_Engine m_engine;

        public AB_Manager_DB()
        {
            m_engine = new EDP_Db_Engine();
        }

        public EDP_Db_Engine Engine => m_engine;

        // Loop tick 매개 호출. 매 tick 단일 직렬화 (engine 안 _syncLock) 매개 dirty entry 파일 flush.
        // 본 group skeleton = 내부 트랜잭셔널 큐 drain hook 자리 — 큐 본체 = 별도 그룹.
        public void SyncDirtyToFile()
        {
            m_engine.SyncDirtyToFile();
        }

        public override void Dispose()
        {
        }
    }
}
