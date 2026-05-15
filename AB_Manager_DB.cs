using System;
using System.Collections.Concurrent;
using System.IO;
using ArtificialBuilder;
using ArtificialBuilder.Common.Base;
using EDPFW;

namespace ArtificialBuilder.DB
{
    // 모듈 최상위 매니저. AB_Manager (root 4) 후손 — 별도 생존주기 / 풀 X / Loop 등록 X. DI Container singleton 매개 lookup.
    // EDP_Db_Engine 단일 보유. 프로그램 단위 lifetime. DB = file SQLite 단일 — 도메인별 / 멀티 머신 / sharding 분리 X (사용자 정본 2026-05-15).
    // DbContext (Microsoft.EntityFrameworkCore v10.0.0) = 본 매니저 내부 전용 (AB_Context_DB). 외부 노출 X. 단일 인스턴스 매개 모든 entity DbSet 망라.
    //   MS Learn 정본: "DbContext instance represents a session with the database... Unit Of Work + Repository patterns".
    //   threading: "EF Core does not support multiple parallel operations being run on the same DbContext instance" — entry.Lock (SemaphoreSlim) 매개 직렬화.
    // CRUD 작업 = 외부 caller 가 EnqueueRequest 호출 → 내부 ConcurrentQueue 보관 → 다음 tick (e) 단계에서 DrainQueue 매개 트랜잭셔널 직렬 처리.
    //   DrainQueue = BeginTransactionAsync 매개 단일 트랜잭션 wrap → 큐 안 모든 envelope 직렬 처리 → CommitAsync 매개 1 회 SaveChanges + dirty 마킹.
    //   처리 안 throw = 트랜잭션 dispose (ChangeTracker.Clear 매개 rollback) + 잔여 envelope 풀 반환 후 본 tick 종료.
    //   본 그룹 = skeleton (handle / 트랜잭션 wrap / 3 도메인 처리 split stub — App / Persona / Package). 각 Command_Type 의 실 EF Core 호출 본체 = 도메인 entity DbSet 정의 후 (별도 그룹).
    //   storage-policy 2026-05-16 정본 매개 Circuit / Logic / Response_UI = JSON 이전 매개 본 매니저 제외.
    public class AB_Manager_DB : AB_Manager
    {
        private readonly EDP_Db_Engine m_engine;

        // 내부 트랜잭셔널 큐. 외부 caller 가 EnqueueRequest 매개 추가. DrainQueue 매개 직렬 처리.
        private readonly ConcurrentQueue<AB_Object_DB_Request_Envelope> m_queue;

        // Envelope pool 매니저 ref — EnqueueRequest 시점 Acquire / DrainQueue 시점 Release 반환.
        private AB_Manager_Object_Instance_Pool? m_pool_manager;

        // EDP_Db_Engine 발급 핸들. OpenDatabase 매개 1 회 발급. 0 = 미부착 (Crash-First).
        private int m_handle;

        public AB_Manager_DB()
        {
            m_engine = new EDP_Db_Engine();
            m_queue = new ConcurrentQueue<AB_Object_DB_Request_Envelope>();
            m_handle = 0;
        }

        public EDP_Db_Engine Engine => m_engine;

        public int Handle => m_handle;

        // Program.cs 부트 시점 호출. Pool Manager 등록 후 envelope pool dispense 가능 상태 진입.
        public void AttachPoolManager(AB_Manager_Object_Instance_Pool _pool_manager)
        {
            m_pool_manager = _pool_manager;
        }

        // Program.cs 부트 시점 호출 (AttachPoolManager 후). 단일 DB 파일 open → handle 발급.
        // 기존 파일 = LoadFileToMemory 매개 메모리 복원 / 없음 = EnsureCreated 매개 신규.
        // 본 매니저 lifetime 단일 호출 — 재호출 = Crash-First throw.
        public void OpenDatabase(string _file_path)
        {
            if (m_handle != 0)
            {
                throw new InvalidOperationException("AB_Manager_DB.OpenDatabase: 이미 발급된 handle=" + m_handle);
            }
            string? dir = Path.GetDirectoryName(_file_path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            int handle = m_engine.OpenDatabase<AB_Context_DB>(_file_path, _opts => new AB_Context_DB(_opts));
            if (handle == 0)
            {
                throw new InvalidOperationException("AB_Manager_DB.OpenDatabase: handle 발급 실패 file=" + _file_path);
            }
            m_handle = handle;
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

        // AB_Object_Loop.Tick (e-1) 단계 매개 매 tick 호출. 큐 안 모든 요청 batch drain → 단일 트랜잭션 wrap → 직렬 처리.
        //   큐 empty = no-op (트랜잭션 미 begin).
        //   BeginTransactionAsync = entry.Lock (SemaphoreSlim) 매개 직렬화 → 본 매개 안 다른 EF Core 작업 진입 X.
        //   처리 안 throw = ChangeTracker.Clear (rollback) + 잔여 envelope 풀 반환 후 throw 재던지 (Crash-First).
        //   CommitAsync = 1 회 SaveChangesAsync + Engine.MarkDirty 매개 다음 SyncDirtyToFile 대상.
        public void DrainQueue()
        {
            if (m_pool_manager == null)
            {
                return;
            }
            if (m_queue.IsEmpty)
            {
                return;
            }
            if (m_handle == 0)
            {
                throw new InvalidOperationException("AB_Manager_DB.DrainQueue: handle 미부착 — OpenDatabase 선 호출");
            }
            EDP_Db_Transaction txn = m_engine.BeginTransactionAsync(m_handle).GetAwaiter().GetResult();
            bool committed = false;
            try
            {
                while (m_queue.TryDequeue(out AB_Object_DB_Request_Envelope? env) && env != null)
                {
                    try
                    {
                        HandleRequest(txn, env);
                    }
                    finally
                    {
                        env.Reset();
                        m_pool_manager.Release(env);
                    }
                }
                txn.CommitAsync().GetAwaiter().GetResult();
                committed = true;
            }
            finally
            {
                if (!committed)
                {
                    txn.DisposeAsync().GetAwaiter().GetResult();
                }
            }
        }

        // 1 요청 처리. CommandType 매개 3 도메인 처리 메서드 분기 (App / Persona / Package).
        //   본 skeleton 단계 = 각 도메인 처리 = case stub body (entity 본체 X / 실 EF Core 호출 X).
        //   entity 정의 후 case 본체 = txn.AddAsync / GetByIdAsync / Update / Remove 매개 채움.
        //   None / End / 미등록 case = Crash-First throw.
        //   storage-policy 2026-05-16 정본 매개 Circuit / Logic / Response_UI = JSON 이전. 본 분기 제외.
        //   Package prefix = entity 스키마 결재 후 enum 항목 + 본 분기 동시 신설 ([[feedback_no_cut_only_stub]]).
        private void HandleRequest(EDP_Db_Transaction _txn, AB_Object_DB_Request_Envelope _env)
        {
            AB_DB_Command_Type cmd = _env.CommandType;
            if (cmd == AB_DB_Command_Type.None)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleRequest: None CommandType 위반");
            }
            if (cmd == AB_DB_Command_Type.End)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleRequest: End CommandType 위반");
            }
            string name = cmd.ToString();
            if (name.StartsWith("APP_DB_"))
            {
                HandleAppDb(_txn, _env);
                return;
            }
            if (name.StartsWith("PERSONA_DB_"))
            {
                HandlePersonaDb(_txn, _env);
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleRequest: 미등록 도메인 prefix command=" + name);
        }

        // 도메인 App DB 처리. APP_DB_MODEL_* 5 case (GET_ALL / GET / ADD / SAVE / DELETE).
        // 본 skeleton = 모든 case stub. entity (예: AB_App_Model) DbSet 정의 후 본체.
        private void HandleAppDb(EDP_Db_Transaction _txn, AB_Object_DB_Request_Envelope _env)
        {
            // skeleton — entity DbSet 정의 후 본체.
        }

        // 도메인 Persona DB 처리. PERSONA_DB_LOAD_ACTIVE 1 case.
        private void HandlePersonaDb(EDP_Db_Transaction _txn, AB_Object_DB_Request_Envelope _env)
        {
            // skeleton — entity DbSet 정의 후 본체.
        }

        // Loop tick 매개 호출. 매 tick 단일 직렬화 (engine 안 _syncLock) 매개 dirty entry 파일 flush.
        public void SyncDirtyToFile()
        {
            m_engine.SyncDirtyToFile();
        }

        public override void Dispose()
        {
            if (m_handle != 0)
            {
                m_engine.CloseAsync(m_handle).GetAwaiter().GetResult();
                m_handle = 0;
            }
            m_engine.DisposeAsync().GetAwaiter().GetResult();
            m_pool_manager = null;
        }
    }
}
