using System;
using System.Collections.Concurrent;
using ArtificialBuilder;
using ArtificialBuilder.Common.Base;
using EDPFW;

namespace ArtificialBuilder.DB
{
    // 모듈 최상위 매니저. AB_Manager (root 4) 후손 — 별도 생존주기 / 풀 X / Loop 등록 X. DI Container singleton 매개 lookup.
    // EDP_Db_Engine 단일 보유. 프로그램 단위 lifetime. DB = file SQLite 단일 — 도메인별 / 멀티 머신 / sharding 분리 X (사용자 정본 2026-05-15).
    // DbContext (Microsoft.EntityFrameworkCore v10.0.0) = 본 매니저 내부 전용. 외부 노출 X. 단일 인스턴스 매개 모든 entity DbSet 망라.
    //   MS Learn 정본: "DbContext instance represents a session with the database... Unit Of Work + Repository patterns".
    //   threading: "EF Core does not support multiple parallel operations being run on the same DbContext instance" — entry.Lock (SemaphoreSlim) 매개 직렬화.
    // CRUD 작업 = 외부 caller 가 EnqueueRequest 호출 → 내부 ConcurrentQueue 보관 → 다음 tick (e) 단계에서 DrainQueue 매개 트랜잭셔널 직렬 처리.
    //   본 그룹 = skeleton (envelope + 큐 + Enqueue + DrainQueue stub). 각 Command_Type 의 실 EF Core 호출 본체 / 실 transaction wrapping = entity DbSet 정의 후 (별도 그룹).
    public class AB_Manager_DB : AB_Manager
    {
        private readonly EDP_Db_Engine m_engine;

        // 내부 트랜잭셔널 큐. 외부 caller 가 EnqueueRequest 매개 추가. DrainQueue 매개 직렬 처리.
        private readonly ConcurrentQueue<AB_Object_DB_Request_Envelope> m_queue;

        // Envelope pool 매니저 ref — EnqueueRequest 시점 Acquire / DrainQueue 시점 Release 반환.
        private AB_Manager_Object_Instance_Pool? m_pool_manager;

        public AB_Manager_DB()
        {
            m_engine = new EDP_Db_Engine();
            m_queue = new ConcurrentQueue<AB_Object_DB_Request_Envelope>();
        }

        public EDP_Db_Engine Engine => m_engine;

        // Program.cs 부트 시점 호출. Pool Manager 등록 후 envelope pool dispense 가능 상태 진입.
        public void AttachPoolManager(AB_Manager_Object_Instance_Pool _pool_manager)
        {
            m_pool_manager = _pool_manager;
        }

        // 외부 caller 매개 DB 요청 큐 enqueue.
        //   _command_type = CRUD / 도메인 분류 (AB_DB_Command_Type)
        //   _data_key = 입력 데이터 id (저장 / 조회 키)
        //   _target_data_id = 출력 데이터 id (Read 결과 슬롯). Read 외는 0
        // Pool Manager 매개 envelope Acquire → field 채움 → m_queue.Enqueue.
        public void EnqueueRequest(AB_DB_Command_Type _command_type, long _data_key, long _target_data_id)
        {
            if (m_pool_manager == null)
            {
                throw new InvalidOperationException("AB_Manager_DB.EnqueueRequest: Pool Manager 미부착 — AttachPoolManager 선 호출");
            }
            AB_Object_DB_Request_Envelope? env = null;
            m_pool_manager.Acquire(ref env);
            env!.CommandType = _command_type;
            env.DataKey = _data_key;
            env.TargetDataId = _target_data_id;
            m_queue.Enqueue(env);
        }

        // AB_Object_Loop.Tick (e) 단계에서 매 tick 호출. 큐 안 모든 요청 batch drain → 트랜잭셔널 직렬 처리.
        //   본 skeleton 단계 = switch(CommandType) 분기 stub body (entity DbSet 정의 후 본체).
        //   실 transaction wrapping (BeginTransactionAsync) = entity / handle 발급 후 활성.
        //   drain 후 envelope 풀 반환.
        public void DrainQueue()
        {
            if (m_pool_manager == null)
            {
                return;
            }
            while (m_queue.TryDequeue(out AB_Object_DB_Request_Envelope? env) && env != null)
            {
                DispatchRequest(env);
                env.Reset();
                m_pool_manager.Release(env);
            }
        }

        // 1 요청 dispatch. switch(CommandType) 분기.
        //   본 skeleton 단계 = 모든 case stub (entity 본체 X / 실 EF Core 호출 X).
        //   entity 정의 후 case 본체 = engine API (FindAsync / AddAsync / Update / Remove / SaveChangesAsync) 매개 채움.
        //   None / End / 미등록 case = Crash-First throw.
        private void DispatchRequest(AB_Object_DB_Request_Envelope _env)
        {
            switch (_env.CommandType)
            {
                case AB_DB_Command_Type.None:
                    throw new InvalidOperationException("AB_Manager_DB.DispatchRequest: None CommandType 위반");
                case AB_DB_Command_Type.End:
                    throw new InvalidOperationException("AB_Manager_DB.DispatchRequest: End CommandType 위반");
                default:
                    // skeleton — 각 case 본체 = entity DbSet 정의 후 (별도 그룹).
                    break;
            }
        }

        // Loop tick 매개 호출. 매 tick 단일 직렬화 (engine 안 _syncLock) 매개 dirty entry 파일 flush.
        public void SyncDirtyToFile()
        {
            m_engine.SyncDirtyToFile();
        }

        public override void Dispose()
        {
            m_pool_manager = null;
        }
    }
}
