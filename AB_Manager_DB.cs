using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ArtificialBuilder;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Data;
using ArtificialBuilder.DB.Object;
using EDPFW;

namespace ArtificialBuilder.DB
{
    // 모듈 최상위 매니저. AB_Manager (root 4) 후손 — 별도 생존주기 / 풀 X / Loop 등록 X. DI Container singleton 매개 lookup.
    // EDP_Db_Engine 단일 보유. 프로그램 단위 lifetime. DB = file SQLite 단일 — 도메인별 / 멀티 머신 / sharding 분리 X (사용자 정본 2026-05-15).
    // DbContext (Microsoft.EntityFrameworkCore v10.0.0) = 본 매니저 내부 전용 (AB_Context_DB). 외부 노출 X. 단일 인스턴스 매개 모든 entity DbSet 망라.
    //   MS Learn 정본: "DbContext instance represents a session with the database... Unit Of Work + Repository patterns".
    //   threading: "EF Core does not support multiple parallel operations being run on the same DbContext instance" — entry.Lock (SemaphoreSlim) 매개 직렬화.
    // CRUD 작업 = 외부 caller 가 EnqueueRequest 호출 → 내부 ConcurrentQueue 보관 → 다음 tick (e) 단계에서 DrainQueue 매개 트랜잭셔널 직렬 처리.
    //   EnqueueRequest = 도메인별 typed overload 3 (App / Persona / Package). caller = typed enum 매개 호출 → 내부 (int)cast 매개 envelope.CommandId 채움 (사용자 정본 2026-05-16 = 전송 정수형).
    //   DrainQueue = BeginTransactionAsync 매개 단일 트랜잭션 wrap → 큐 안 모든 envelope 직렬 처리 → CommitAsync 매개 1 회 SaveChanges + dirty 마킹 → post-commit NotifyDataKey 발화.
    //   처리 안 throw = 트랜잭션 dispose (ChangeTracker.Clear 매개 rollback) + 잔여 envelope 풀 반환 + notify X (Crash-First).
    //   storage-policy 2026-05-16 정본 매개 Circuit / Logic / Response_UI = JSON 이전 매개 본 매니저 제외.
    //
    // 칠판 ↔ EF entity 흐름 (db-app-entity-body 정합 2026-05-16):
    //   envelope.DataKey = 입력 칠판 slot DataId (entity Id 또는 entity 본체)
    //   envelope.TargetDataId = 출력 칠판 slot DataId (Read 결과). Read 외 = 0
    //   handler 매개 m_blackboard.Lookup<T> 입력 read / Lookup<T>.Set 출력 write
    //   NotifyDataKey 발화 = post-commit 단일 점 — HashSet<long> 매개 DrainQueue 안 모음 → CommitAsync 성공 후 발화 ([[feedback_call_via_di_queue]] 정합)
    public class AB_Manager_DB : AB_Manager
    {
        private readonly EDP_Db_Engine m_engine;

        // 내부 트랜잭셔널 큐. 외부 caller 가 EnqueueRequest 매개 추가. DrainQueue 매개 직렬 처리.
        private readonly ConcurrentQueue<AB_Object_DB_Request_Envelope> m_queue;

        // Envelope pool 매니저 ref — EnqueueRequest 시점 Acquire / DrainQueue 시점 Release 반환.
        private AB_Manager_Object_Instance_Pool? m_pool_manager;

        // 칠판 ref — handler 매개 입력 read / 출력 write + post-commit NotifyDataKey 발화 site.
        private AB_Object_Blackboard? m_blackboard;

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

        // Program.cs 부트 시점 호출 (AttachPoolManager 후 / OpenDatabase 전). 칠판 ref 등록 매개 handler 안 Lookup / Set + post-commit NotifyDataKey 발화 가능 상태 진입.
        public void AttachBlackboard(AB_Object_Blackboard _blackboard)
        {
            m_blackboard = _blackboard;
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

        // App 도메인 DB 요청 큐 enqueue. caller = AB_DB_App_Command_Type typed enum 매개 호출.
        //   _command = App 도메인 커맨드 (typed enum) → 내부 (int)cast 매개 envelope.CommandId 채움 (전송 정수형)
        //   _data_key = 입력 데이터 id (저장 / 조회 키)
        //   _target_data_id = 출력 데이터 id (Read 결과 슬롯). Read 외는 0
        public void EnqueueRequest(AB_DB_App_Command_Type _command, long _data_key, long _target_data_id)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.App, (int)_command, _data_key, _target_data_id);
        }

        // Persona 도메인 DB 요청 큐 enqueue. caller = AB_DB_Persona_Command_Type typed enum 매개 호출.
        public void EnqueueRequest(AB_DB_Persona_Command_Type _command, long _data_key, long _target_data_id)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.Persona, (int)_command, _data_key, _target_data_id);
        }

        // Package 도메인 DB 요청 큐 enqueue. caller = AB_DB_Package_Command_Type typed enum 매개 호출.
        public void EnqueueRequest(AB_DB_Package_Command_Type _command, long _data_key, long _target_data_id)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.Package, (int)_command, _data_key, _target_data_id);
        }

        // 도메인별 typed overload 3 의 단일 내부 진입. Pool Manager 매개 envelope Acquire → field 채움 → m_queue.Enqueue.
        private void EnqueueRequestInternal(AB_DB_Domain_Kind _domain, int _command_id, long _data_key, long _target_data_id)
        {
            if (m_pool_manager == null)
            {
                throw new InvalidOperationException("AB_Manager_DB.EnqueueRequest: Pool Manager 미부착 — AttachPoolManager 선 호출");
            }
            AB_Object_DB_Request_Envelope? env = null;
            m_pool_manager.Acquire(ref env);
            env!.DomainKind = _domain;
            env.CommandId = _command_id;
            env.DataKey = _data_key;
            env.TargetDataId = _target_data_id;
            m_queue.Enqueue(env);
        }

        // AB_Object_Loop.Tick (e-1) 단계 매개 매 tick 호출. 큐 안 모든 요청 batch drain → 단일 트랜잭션 wrap → 직렬 처리.
        //   큐 empty = no-op (트랜잭션 미 begin).
        //   BeginTransactionAsync = entry.Lock (SemaphoreSlim) 매개 직렬화 → 본 매개 안 다른 EF Core 작업 진입 X.
        //   처리 안 throw = ChangeTracker.Clear (rollback) + 잔여 envelope 풀 반환 후 throw 재던지 (Crash-First). notify_ids 발화 X.
        //   CommitAsync 성공 후 notify_ids HashSet<long> 매개 칠판 NotifyDataKey 발화 — 옵저버 측 메시지 큐 enqueue ([[feedback_call_via_di_queue]] 정합).
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
            HashSet<long> notify_ids = new HashSet<long>();
            bool committed = false;
            try
            {
                while (m_queue.TryDequeue(out AB_Object_DB_Request_Envelope? env) && env != null)
                {
                    try
                    {
                        HandleRequest(txn, env);
                        if (env.TargetDataId != 0)
                        {
                            notify_ids.Add(env.TargetDataId);
                        }
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
            if (m_blackboard != null)
            {
                foreach (long id in notify_ids)
                {
                    m_blackboard.NotifyDataKey(id);
                }
            }
        }

        // 1 요청 처리. envelope.DomainKind 매개 3 도메인 handler 분기 (App / Persona / Package).
        //   각 handler = (PerDomainEnum)envelope.CommandId cast 매개 switch.
        //   None / End / 미등록 DomainKind = Crash-First throw.
        //   storage-policy 2026-05-16 정본 매개 Circuit / Logic / Response_UI = JSON 이전. 본 분기 제외.
        private void HandleRequest(EDP_Db_Transaction _txn, AB_Object_DB_Request_Envelope _env)
        {
            AB_DB_Domain_Kind domain = _env.DomainKind;
            if (domain == AB_DB_Domain_Kind.None)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleRequest: None DomainKind 위반");
            }
            if (domain == AB_DB_Domain_Kind.End)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleRequest: End DomainKind 위반");
            }
            if (domain == AB_DB_Domain_Kind.App)
            {
                HandleAppDb(_txn, (AB_DB_App_Command_Type)_env.CommandId, _env);
                return;
            }
            if (domain == AB_DB_Domain_Kind.Persona)
            {
                HandlePersonaDb(_txn, (AB_DB_Persona_Command_Type)_env.CommandId, _env);
                return;
            }
            if (domain == AB_DB_Domain_Kind.Package)
            {
                HandlePackageDb(_txn, (AB_DB_Package_Command_Type)_env.CommandId, _env);
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleRequest: 미등록 DomainKind=" + domain);
        }

        // 도메인 App DB 처리. AB_DB_App_Command_Type 5 case 본체 (MODEL_GET_ALL / GET / ADD / SAVE / DELETE).
        //   slot 타입: MODEL_GET_ALL TargetDataId = AB_Data_DB_App_Model_List / MODEL_GET DataKey = AB_Data_Long, TargetDataId = AB_Data_DB_App_Model
        //              MODEL_ADD / SAVE DataKey = AB_Data_DB_App_Model / MODEL_DELETE DataKey = AB_Data_Long
        //   CREDENTIAL_* = 암호화 키 운영 결재 후 별도 그룹 (db-credential-encryption) 매개 신설 ([[feedback_no_cut_only_stub]]).
        private void HandleAppDb(EDP_Db_Transaction _txn, AB_DB_App_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            if (m_blackboard == null)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleAppDb: Blackboard 미부착 — AttachBlackboard 선 호출");
            }
            if (_command == AB_DB_App_Command_Type.None)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleAppDb: None Command 위반");
            }
            if (_command == AB_DB_App_Command_Type.End)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleAppDb: End Command 위반");
            }
            if (_command == AB_DB_App_Command_Type.MODEL_GET_ALL)
            {
                List<AB_Object_DB_App_Model> all = _txn.GetAllAsync<AB_Object_DB_App_Model>().GetAwaiter().GetResult();
                AB_Data_DB_App_Model_List target = m_blackboard.Lookup<AB_Data_DB_App_Model_List>(_env.TargetDataId);
                target.Set(all);
                return;
            }
            if (_command == AB_DB_App_Command_Type.MODEL_GET)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                long id = input_id.Get();
                AB_Object_DB_App_Model? entity = _txn.GetByIdAsync<AB_Object_DB_App_Model>(id).GetAwaiter().GetResult();
                AB_Data_DB_App_Model target = m_blackboard.Lookup<AB_Data_DB_App_Model>(_env.TargetDataId);
                target.Set(entity);
                return;
            }
            if (_command == AB_DB_App_Command_Type.MODEL_ADD)
            {
                AB_Data_DB_App_Model input = m_blackboard.Lookup<AB_Data_DB_App_Model>(_env.DataKey);
                AB_Object_DB_App_Model? entity = input.Get();
                if (entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleAppDb.MODEL_ADD: 입력 entity null — DataKey=" + _env.DataKey);
                }
                _txn.AddAsync(entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_App_Command_Type.MODEL_SAVE)
            {
                AB_Data_DB_App_Model input = m_blackboard.Lookup<AB_Data_DB_App_Model>(_env.DataKey);
                AB_Object_DB_App_Model? entity = input.Get();
                if (entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleAppDb.MODEL_SAVE: 입력 entity null — DataKey=" + _env.DataKey);
                }
                _txn.Update(entity);
                return;
            }
            if (_command == AB_DB_App_Command_Type.MODEL_DELETE)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                long id = input_id.Get();
                AB_Object_DB_App_Model? entity = _txn.GetByIdAsync<AB_Object_DB_App_Model>(id).GetAwaiter().GetResult();
                if (entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleAppDb.MODEL_DELETE: 삭제 대상 entity 미존재 id=" + id);
                }
                _txn.Remove(entity);
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleAppDb: 미등록 Command=" + _command);
        }

        // 도메인 Persona DB 처리. AB_DB_Persona_Command_Type 1 case (LOAD_ACTIVE).
        // 본 skeleton = 모든 case stub. handler 본체 = queue #5 db-persona-entity-body 매개 채움.
        private void HandlePersonaDb(EDP_Db_Transaction _txn, AB_DB_Persona_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            // skeleton — queue #5 db-persona-entity-body 매개 본체.
        }

        // 도메인 Package DB 처리. AB_DB_Package_Command_Type 5 case (INFO_GET_ALL / GET / ADD / SAVE / DELETE).
        // 본 skeleton = 모든 case stub. handler 본체 = queue #6 db-package-entity-body 매개 채움.
        private void HandlePackageDb(EDP_Db_Transaction _txn, AB_DB_Package_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            // skeleton — queue #6 db-package-entity-body 매개 본체.
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
            m_blackboard = null;
        }
    }
}
