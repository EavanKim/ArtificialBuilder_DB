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
    // EDP_Db_Engine 단일 보유. 프로그램 단위 lifetime.
    // round 3 (2026-05-17) — 5 도메인 (App / Persona / Package / Turn / Result) 매개 3 Object 등록:
    //   Normal           = asset.db 단일 파일 (App / Persona / Package)
    //   Sharding_History = `{root}/turn/` 폴더 안 shard_key (= turn_id / 100) 파일 N (Turn)
    //   Sharding_Key     = `{root}/result/` 폴더 안 shard_key (= data_key / 100) 파일 N (Result)
    //   round 3 = 폴더 = root flat. per-circuit / per-node 폴더 분기 = round 4 (interpreter wiring) 매개 — 본 round 비범위.
    //
    // CRUD 작업 = 외부 caller 가 EnqueueRequest 호출 → 내부 ConcurrentQueue 보관 → 다음 tick (e) 단계에서 DrainQueue 매개 트랜잭셔널 직렬 처리.
    //   EnqueueRequest = 도메인별 typed overload 5 (App / Persona / Package + Turn / Result). Turn / Result = shard_key 인자 필수 (W1=a caller 산정).
    //   DrainQueue = 큐 안 envelope 들을 (Object_Kind, shard_key) 매개 그룹핑 → 그룹별 1 txn → 그룹 안 envelope 직렬 처리 → CommitAsync → post-commit NotifyDataKey 발화.
    //     Normal 도메인 = ShardKey 무시 (0 그룹 단일). Sharding 도메인 = shard_key 매개 별도 txn (1 shard = 1 파일 = 1 txn).
    //   처리 안 throw = 트랜잭션 dispose (rollback) + 잔여 envelope 풀 반환 + notify X (Crash-First).
    //
    // 칠판 ↔ EF entity 흐름:
    //   envelope.DataKey = 입력 칠판 slot DataId / envelope.TargetDataId = 출력 칠판 slot DataId / envelope.ShardKey = Sharding 시 caller 산정 shard_key
    //   handler 매개 m_blackboard.Lookup<T> 입력 read / Lookup<T>.Set 출력 write
    //   NotifyDataKey 발화 = post-commit 단일 점 — HashSet<long> 매개 그룹 별 DrainQueue 안 모음 → CommitAsync 성공 후 발화 ([[feedback_call_via_di_queue]] 정합)
    public class AB_Manager_DB : AB_Manager
    {
        private readonly EDP_Db_Engine m_engine;

        // 내부 트랜잭셔널 큐. 외부 caller 가 EnqueueRequest 매개 추가. DrainQueue 매개 직렬 처리.
        private readonly ConcurrentQueue<AB_Object_DB_Request_Envelope> m_queue;

        // Envelope pool 매니저 ref — EnqueueRequest 시점 Acquire / DrainQueue 시점 Release 반환.
        private AB_Manager_Object_Instance_Pool? m_pool_manager;

        // 칠판 ref — handler 매개 입력 read / 출력 write + post-commit NotifyDataKey 발화 site.
        private AB_Object_Blackboard? m_blackboard;

        // 다형성 dispatch 등록부. 매니저는 AB_Object_DB abstract 만 매개 동작 — 콘크리트 노출 X.
        // round 3 (2026-05-17) — Normal + Sharding_History (Turn) + Sharding_Key (Result) 3 콘크리트 등록.
        private readonly Dictionary<AB_DB_Object_Kind, AB_Object_DB> m_db_objects;

        public AB_Manager_DB()
        {
            m_engine = new EDP_Db_Engine();
            m_queue = new ConcurrentQueue<AB_Object_DB_Request_Envelope>();
            m_db_objects = new Dictionary<AB_DB_Object_Kind, AB_Object_DB>();
        }

        public void AttachPoolManager(AB_Manager_Object_Instance_Pool _pool_manager)
        {
            m_pool_manager = _pool_manager;
        }

        public void AttachBlackboard(AB_Object_Blackboard _blackboard)
        {
            m_blackboard = _blackboard;
        }

        // Program.cs 부트 시점 호출 (AttachPoolManager 후). 본 매니저 lifetime 단일 호출.
        // root 1 = 모든 DB 자산 폴더. 안에:
        //   `{root}/asset.db`  = Normal (App / Persona / Package)
        //   `{root}/turn/`     = Sharding_History (Turn)
        //   `{root}/result/`   = Sharding_Key (Result)
        // per-circuit / per-node 폴더 (storage-policy round 2 § "Turn / Result 폴더") 분기 = round 4 매개 — 본 round = root flat.
        public void OpenDatabase(string _root_dir)
        {
            Directory.CreateDirectory(_root_dir);

            string asset_db = Path.Combine(_root_dir, "asset.db");
            AB_Object_DB_Normal normal = new AB_Object_DB_Normal();
            RegisterDbObject_(AB_DB_Object_Kind.Normal, normal);
            normal.Open_(asset_db, m_engine);

            string turn_root = Path.Combine(_root_dir, "turn");
            AB_Object_DB_Sharding_History turn_shard = new AB_Object_DB_Sharding_History();
            RegisterDbObject_(AB_DB_Object_Kind.Sharding_History, turn_shard);
            turn_shard.Open_(turn_root, m_engine);

            string result_root = Path.Combine(_root_dir, "result");
            AB_Object_DB_Sharding_Key result_shard = new AB_Object_DB_Sharding_Key();
            RegisterDbObject_(AB_DB_Object_Kind.Sharding_Key, result_shard);
            result_shard.Open_(result_root, m_engine);
        }

        public void RegisterDbObject_(AB_DB_Object_Kind _kind, AB_Object_DB _db_object)
        {
            if (_kind == AB_DB_Object_Kind.None)
            {
                throw new InvalidOperationException("AB_Manager_DB.RegisterDbObject_: None Kind 위반");
            }
            if (_kind == AB_DB_Object_Kind.End)
            {
                throw new InvalidOperationException("AB_Manager_DB.RegisterDbObject_: End Kind 위반");
            }
            if (_db_object == null)
            {
                throw new InvalidOperationException("AB_Manager_DB.RegisterDbObject_: db_object null Kind=" + _kind);
            }
            if (m_db_objects.ContainsKey(_kind))
            {
                throw new InvalidOperationException("AB_Manager_DB.RegisterDbObject_: 중복 등록 Kind=" + _kind);
            }
            m_db_objects[_kind] = _db_object;
        }

        public AB_Object_DB Dispatch_(AB_DB_Object_Kind _kind)
        {
            if (_kind == AB_DB_Object_Kind.None)
            {
                throw new InvalidOperationException("AB_Manager_DB.Dispatch_: None Kind 위반");
            }
            if (_kind == AB_DB_Object_Kind.End)
            {
                throw new InvalidOperationException("AB_Manager_DB.Dispatch_: End Kind 위반");
            }
            if (!m_db_objects.TryGetValue(_kind, out AB_Object_DB? db))
            {
                throw new InvalidOperationException("AB_Manager_DB.Dispatch_: 미등록 Kind=" + _kind);
            }
            return db;
        }

        // === EnqueueRequest typed overload 5 ===

        public void EnqueueRequest(AB_DB_App_Command_Type _command, long _data_key, long _target_data_id)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.App, (int)_command, _data_key, _target_data_id, 0L);
        }

        public void EnqueueRequest(AB_DB_Persona_Command_Type _command, long _data_key, long _target_data_id)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.Persona, (int)_command, _data_key, _target_data_id, 0L);
        }

        public void EnqueueRequest(AB_DB_Package_Command_Type _command, long _data_key, long _target_data_id)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.Package, (int)_command, _data_key, _target_data_id, 0L);
        }

        public void EnqueueRequest(AB_DB_Circuit_Command_Type _command, long _data_key, long _target_data_id)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.Circuit, (int)_command, _data_key, _target_data_id, 0L);
        }

        public void EnqueueRequest(AB_DB_Logic_Command_Type _command, long _data_key, long _target_data_id)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.Logic, (int)_command, _data_key, _target_data_id, 0L);
        }

        public void EnqueueRequest(AB_DB_Model_Command_Type _command, long _data_key, long _target_data_id)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.Model, (int)_command, _data_key, _target_data_id, 0L);
        }

        // Turn 도메인 — shard_key 인자 필수 (= turn_id / 100, caller 산정 W1=a).
        public void EnqueueRequest(AB_DB_Turn_Command_Type _command, long _data_key, long _target_data_id, long _shard_key)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.Turn, (int)_command, _data_key, _target_data_id, _shard_key);
        }

        // Result 도메인 — shard_key 인자 필수 (= data_key / 100, caller 산정 W1=a).
        public void EnqueueRequest(AB_DB_Result_Command_Type _command, long _data_key, long _target_data_id, long _shard_key)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.Result, (int)_command, _data_key, _target_data_id, _shard_key);
        }

        public void EnqueueRequest(AB_DB_ChatTurn_Command_Type _command, long _data_key, long _target_data_id)
        {
            EnqueueRequestInternal(AB_DB_Domain_Kind.ChatTurn, (int)_command, _data_key, _target_data_id, 0L);
        }

        private void EnqueueRequestInternal(AB_DB_Domain_Kind _domain, int _command_id, long _data_key, long _target_data_id, long _shard_key)
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
            env.ShardKey = _shard_key;
            m_queue.Enqueue(env);
        }

        // === DrainQueue — 다중 txn 그룹 처리 (round 3) ===

        // AB_Manager_Engine.Tick (e-1) 매개 매 tick 호출. 큐 안 모든 envelope batch drain → (Object_Kind, shard_key) 그룹핑 → 그룹별 1 txn 매개 직렬 처리 → 그룹별 commit.
        //   Normal 도메인 = ShardKey 무시 (0 그룹 단일).
        //   Sharding 도메인 = shard_key 매개 별도 txn (cross-shard 무결성 = 어플리케이션 책임, [[feedback_fk_minimization_speed_customization]] 정합).
        //   그룹 안 throw = 그 그룹 txn rollback + 그 그룹 envelope 풀 반환 + 그 그룹 notify_ids X (다른 그룹 = 정상 commit, 부분 진행 허용 — Crash-First 후 throw 재던지).
        //   CommitAsync 성공 후 그룹별 notify_ids HashSet<long> 매개 칠판 NotifyDataKey 발화.
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

            // 큐 → 그룹 분배. key = (Object_Kind, shard_key). Normal = (Normal, 0) 단일 그룹.
            Dictionary<(AB_DB_Object_Kind, long), List<AB_Object_DB_Request_Envelope>> groups
                = new Dictionary<(AB_DB_Object_Kind, long), List<AB_Object_DB_Request_Envelope>>();
            // 그룹 순서 보존 매개 별도 list (Dictionary key 순서 = 비결정).
            List<(AB_DB_Object_Kind, long)> group_order = new List<(AB_DB_Object_Kind, long)>();

            while (m_queue.TryDequeue(out AB_Object_DB_Request_Envelope? env) && env != null)
            {
                AB_DB_Object_Kind kind = ObjectKindFromDomain_(env.DomainKind);
                long shard = (kind == AB_DB_Object_Kind.Normal) ? 0L : env.ShardKey;
                (AB_DB_Object_Kind, long) key = (kind, shard);
                if (!groups.TryGetValue(key, out List<AB_Object_DB_Request_Envelope>? list))
                {
                    list = new List<AB_Object_DB_Request_Envelope>();
                    groups[key] = list;
                    group_order.Add(key);
                }
                list.Add(env);
            }

            HashSet<long> all_notify_ids = new HashSet<long>();
            foreach ((AB_DB_Object_Kind, long) key in group_order)
            {
                List<AB_Object_DB_Request_Envelope> list = groups[key];
                AB_Object_DB db = Dispatch_(key.Item1);
                EDP_Db_Transaction txn;
                if (key.Item1 == AB_DB_Object_Kind.Normal)
                {
                    txn = db.BeginTransactionAsync_().GetAwaiter().GetResult();
                }
                else
                {
                    txn = db.BeginTransactionAsync_(key.Item2).GetAwaiter().GetResult();
                }
                HashSet<long> notify_ids = new HashSet<long>();
                bool committed = false;
                try
                {
                    foreach (AB_Object_DB_Request_Envelope env in list)
                    {
                        try
                        {
                            HandleRequest(db, txn, env);
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
                foreach (long id in notify_ids)
                {
                    all_notify_ids.Add(id);
                }
            }

            if (m_blackboard != null)
            {
                foreach (long id in all_notify_ids)
                {
                    m_blackboard.NotifyDataKey(id);
                }
            }
        }

        // DomainKind → Object_Kind 매핑. None / End / 미등록 = Crash-First throw.
        private static AB_DB_Object_Kind ObjectKindFromDomain_(AB_DB_Domain_Kind _domain)
        {
            if (_domain == AB_DB_Domain_Kind.None)
            {
                throw new InvalidOperationException("AB_Manager_DB.ObjectKindFromDomain_: None DomainKind 위반");
            }
            if (_domain == AB_DB_Domain_Kind.End)
            {
                throw new InvalidOperationException("AB_Manager_DB.ObjectKindFromDomain_: End DomainKind 위반");
            }
            if (_domain == AB_DB_Domain_Kind.App)
            {
                return AB_DB_Object_Kind.Normal;
            }
            if (_domain == AB_DB_Domain_Kind.Persona)
            {
                return AB_DB_Object_Kind.Normal;
            }
            if (_domain == AB_DB_Domain_Kind.Package)
            {
                return AB_DB_Object_Kind.Normal;
            }
            if (_domain == AB_DB_Domain_Kind.Circuit)
            {
                return AB_DB_Object_Kind.Normal;
            }
            if (_domain == AB_DB_Domain_Kind.Logic)
            {
                return AB_DB_Object_Kind.Normal;
            }
            if (_domain == AB_DB_Domain_Kind.Model)
            {
                return AB_DB_Object_Kind.Normal;
            }
            if (_domain == AB_DB_Domain_Kind.Turn)
            {
                return AB_DB_Object_Kind.Sharding_History;
            }
            if (_domain == AB_DB_Domain_Kind.Result)
            {
                return AB_DB_Object_Kind.Sharding_Key;
            }
            if (_domain == AB_DB_Domain_Kind.ChatTurn)
            {
                return AB_DB_Object_Kind.Normal;
            }
            throw new InvalidOperationException("AB_Manager_DB.ObjectKindFromDomain_: 미등록 DomainKind=" + _domain);
        }

        // 1 요청 처리. envelope.DomainKind 매개 5 도메인 handler 분기.
        //   각 handler = (PerDomainEnum)envelope.CommandId cast 매개 switch.
        //   None / End / 미등록 DomainKind = Crash-First throw.
        private void HandleRequest(AB_Object_DB _db, EDP_Db_Transaction _txn, AB_Object_DB_Request_Envelope _env)
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
                HandleAppDb(_db, _txn, (AB_DB_App_Command_Type)_env.CommandId, _env);
                return;
            }
            if (domain == AB_DB_Domain_Kind.Persona)
            {
                HandlePersonaDb(_db, _txn, (AB_DB_Persona_Command_Type)_env.CommandId, _env);
                return;
            }
            if (domain == AB_DB_Domain_Kind.Package)
            {
                HandlePackageDb(_db, _txn, (AB_DB_Package_Command_Type)_env.CommandId, _env);
                return;
            }
            if (domain == AB_DB_Domain_Kind.Circuit)
            {
                HandleCircuitDb(_db, _txn, (AB_DB_Circuit_Command_Type)_env.CommandId, _env);
                return;
            }
            if (domain == AB_DB_Domain_Kind.Logic)
            {
                HandleLogicDb(_db, _txn, (AB_DB_Logic_Command_Type)_env.CommandId, _env);
                return;
            }
            if (domain == AB_DB_Domain_Kind.Model)
            {
                HandleModelDb(_db, _txn, (AB_DB_Model_Command_Type)_env.CommandId, _env);
                return;
            }
            if (domain == AB_DB_Domain_Kind.Turn)
            {
                HandleTurnDb(_db, _txn, (AB_DB_Turn_Command_Type)_env.CommandId, _env);
                return;
            }
            if (domain == AB_DB_Domain_Kind.Result)
            {
                HandleResultDb(_db, _txn, (AB_DB_Result_Command_Type)_env.CommandId, _env);
                return;
            }
            if (domain == AB_DB_Domain_Kind.ChatTurn)
            {
                HandleChatTurnDb(_db, _txn, (AB_DB_ChatTurn_Command_Type)_env.CommandId, _env);
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleRequest: 미등록 DomainKind=" + domain);
        }

        // === App 도메인 핸들러 (round 2c 그대로) ===
        private void HandleAppDb(AB_Object_DB _db, EDP_Db_Transaction _txn, AB_DB_App_Command_Type _command, AB_Object_DB_Request_Envelope _env)
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
                List<AB_Object_DB_App_Model> all = _db.FindAsync_<AB_Object_DB_App_Model>(_txn, _x => true).GetAwaiter().GetResult();
                AB_Data_DB_App_Model_List target = m_blackboard.Lookup<AB_Data_DB_App_Model_List>(_env.TargetDataId);
                target.Set(all);
                return;
            }
            if (_command == AB_DB_App_Command_Type.MODEL_GET)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                long id = input_id.Get();
                AB_Object_DB_App_Model? entity = _db.GetByIdAsync_<AB_Object_DB_App_Model>(_txn, id).GetAwaiter().GetResult();
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
                _db.AddRowAsync_(_txn, entity).GetAwaiter().GetResult();
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
                _db.UpdateAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_App_Command_Type.MODEL_DELETE)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                long id = input_id.Get();
                AB_Object_DB_App_Model? entity = _db.GetByIdAsync_<AB_Object_DB_App_Model>(_txn, id).GetAwaiter().GetResult();
                if (entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleAppDb.MODEL_DELETE: 삭제 대상 entity 미존재 id=" + id);
                }
                _db.RemoveAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleAppDb: 미등록 Command=" + _command);
        }

        // === Persona 도메인 핸들러 ===
        private void HandlePersonaDb(AB_Object_DB _db, EDP_Db_Transaction _txn, AB_DB_Persona_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            if (m_blackboard == null)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandlePersonaDb: Blackboard 미부착 — AttachBlackboard 선 호출");
            }
            if (_command == AB_DB_Persona_Command_Type.None)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandlePersonaDb: None Command 위반");
            }
            if (_command == AB_DB_Persona_Command_Type.End)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandlePersonaDb: End Command 위반");
            }
            if (_command == AB_DB_Persona_Command_Type.LOAD_ACTIVE)
            {
                List<AB_Object_DB_Persona> actives = _db.FindAsync_<AB_Object_DB_Persona>(_txn, _x => _x.IsActive).GetAwaiter().GetResult();
                AB_Object_DB_Persona? entity = null;
                if (actives.Count > 0)
                {
                    entity = actives[0];
                }
                AB_Data_DB_Persona target = m_blackboard.Lookup<AB_Data_DB_Persona>(_env.TargetDataId);
                target.Set(entity);
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandlePersonaDb: 미등록 Command=" + _command);
        }

        // === Package 도메인 핸들러 ===
        private void HandlePackageDb(AB_Object_DB _db, EDP_Db_Transaction _txn, AB_DB_Package_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            if (m_blackboard == null)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandlePackageDb: Blackboard 미부착 — AttachBlackboard 선 호출");
            }
            if (_command == AB_DB_Package_Command_Type.None)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandlePackageDb: None Command 위반");
            }
            if (_command == AB_DB_Package_Command_Type.End)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandlePackageDb: End Command 위반");
            }
            if (_command == AB_DB_Package_Command_Type.INFO_GET_ALL)
            {
                List<AB_Object_DB_Package> all = _db.FindAsync_<AB_Object_DB_Package>(_txn, _x => true).GetAwaiter().GetResult();
                AB_Data_DB_Package_List target = m_blackboard.Lookup<AB_Data_DB_Package_List>(_env.TargetDataId);
                target.Set(all);
                return;
            }
            if (_command == AB_DB_Package_Command_Type.INFO_GET)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                long id = input_id.Get();
                AB_Object_DB_Package? entity = _db.GetByIdAsync_<AB_Object_DB_Package>(_txn, id).GetAwaiter().GetResult();
                AB_Data_DB_Package target = m_blackboard.Lookup<AB_Data_DB_Package>(_env.TargetDataId);
                target.Set(entity);
                return;
            }
            if (_command == AB_DB_Package_Command_Type.INFO_ADD)
            {
                AB_Data_DB_Package input = m_blackboard.Lookup<AB_Data_DB_Package>(_env.DataKey);
                AB_Object_DB_Package? entity = input.Get();
                if (entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandlePackageDb.INFO_ADD: 입력 entity null — DataKey=" + _env.DataKey);
                }
                _db.AddRowAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Package_Command_Type.INFO_SAVE)
            {
                AB_Data_DB_Package input = m_blackboard.Lookup<AB_Data_DB_Package>(_env.DataKey);
                AB_Object_DB_Package? entity = input.Get();
                if (entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandlePackageDb.INFO_SAVE: 입력 entity null — DataKey=" + _env.DataKey);
                }
                _db.UpdateAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Package_Command_Type.INFO_DELETE)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                long id = input_id.Get();
                AB_Object_DB_Package? entity = _db.GetByIdAsync_<AB_Object_DB_Package>(_txn, id).GetAwaiter().GetResult();
                if (entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandlePackageDb.INFO_DELETE: 삭제 대상 entity 미존재 id=" + id);
                }
                _db.RemoveAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandlePackageDb: 미등록 Command=" + _command);
        }

        private void HandleCircuitDb(AB_Object_DB _db, EDP_Db_Transaction _txn, AB_DB_Circuit_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            if (m_blackboard == null) throw new InvalidOperationException("AB_Manager_DB.HandleCircuitDb: Blackboard 미부착");
            if (_command == AB_DB_Circuit_Command_Type.None) throw new InvalidOperationException("AB_Manager_DB.HandleCircuitDb: None 위반");
            if (_command == AB_DB_Circuit_Command_Type.End) throw new InvalidOperationException("AB_Manager_DB.HandleCircuitDb: End 위반");
            if (_command == AB_DB_Circuit_Command_Type.INFO_GET_ALL)
            {
                List<AB_Object_DB_Circuit> all = _db.FindAsync_<AB_Object_DB_Circuit>(_txn, _x => true).GetAwaiter().GetResult();
                AB_Data_DB_Circuit_List target = m_blackboard.Lookup<AB_Data_DB_Circuit_List>(_env.TargetDataId);
                target.Set(all);
                return;
            }
            if (_command == AB_DB_Circuit_Command_Type.INFO_GET)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                AB_Object_DB_Circuit? entity = _db.GetByIdAsync_<AB_Object_DB_Circuit>(_txn, input_id.Get()).GetAwaiter().GetResult();
                AB_Data_DB_Circuit target = m_blackboard.Lookup<AB_Data_DB_Circuit>(_env.TargetDataId);
                target.Set(entity);
                return;
            }
            if (_command == AB_DB_Circuit_Command_Type.INFO_ADD)
            {
                AB_Object_DB_Circuit? entity = m_blackboard.Lookup<AB_Data_DB_Circuit>(_env.DataKey).Get();
                if (entity == null) throw new InvalidOperationException("AB_Manager_DB.HandleCircuitDb.INFO_ADD: entity null");
                _db.AddRowAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Circuit_Command_Type.INFO_SAVE)
            {
                AB_Object_DB_Circuit? entity = m_blackboard.Lookup<AB_Data_DB_Circuit>(_env.DataKey).Get();
                if (entity == null) throw new InvalidOperationException("AB_Manager_DB.HandleCircuitDb.INFO_SAVE: entity null");
                _db.UpdateAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Circuit_Command_Type.INFO_DELETE)
            {
                long id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey).Get();
                AB_Object_DB_Circuit? entity = _db.GetByIdAsync_<AB_Object_DB_Circuit>(_txn, id).GetAwaiter().GetResult();
                if (entity == null) throw new InvalidOperationException("AB_Manager_DB.HandleCircuitDb.INFO_DELETE: id 미존재=" + id);
                _db.RemoveAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleCircuitDb: 미등록 Command=" + _command);
        }

        // ChatTurn 도메인 핸들러 — chat history 매개 매개 매개 매개 매개 매개 매개 매개 매개.
        //   INFO_ADD               = entity 매개 INSERT (CreatedAt 매개 caller 매개 매개 매개).
        //   INFO_GET_BY_PACKAGE    = data_key 매개 AB_Data_Long (package_id) → AB_Data_DB_Chat_Turn_List 매개 매개 매개 sort by CreatedAt.
        //   INFO_DELETE_BY_PACKAGE = data_key 매개 AB_Data_Long (package_id) → 매개 매개 매개 매개 매개 매개 매개.
        private void HandleChatTurnDb(AB_Object_DB _db, EDP_Db_Transaction _txn, AB_DB_ChatTurn_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            if (m_blackboard == null) throw new InvalidOperationException("AB_Manager_DB.HandleChatTurnDb: Blackboard 미부착");
            if (_command == AB_DB_ChatTurn_Command_Type.None) throw new InvalidOperationException("AB_Manager_DB.HandleChatTurnDb: None 위반");
            if (_command == AB_DB_ChatTurn_Command_Type.End) throw new InvalidOperationException("AB_Manager_DB.HandleChatTurnDb: End 위반");
            if (_command == AB_DB_ChatTurn_Command_Type.INFO_ADD)
            {
                AB_Object_DB_Chat_Turn? entity = m_blackboard.Lookup<AB_Data_DB_Chat_Turn>(_env.DataKey).Get();
                if (entity == null) throw new InvalidOperationException("AB_Manager_DB.HandleChatTurnDb.INFO_ADD: entity null");
                _db.AddRowAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_ChatTurn_Command_Type.INFO_GET_BY_PACKAGE)
            {
                long pkg_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey).Get();
                List<AB_Object_DB_Chat_Turn> rows = _db.FindAsync_<AB_Object_DB_Chat_Turn>(_txn, _x => _x.PackageId == pkg_id).GetAwaiter().GetResult();
                rows.Sort((_a, _b) => _a.CreatedAt.CompareTo(_b.CreatedAt));
                AB_Data_DB_Chat_Turn_List target = m_blackboard.Lookup<AB_Data_DB_Chat_Turn_List>(_env.TargetDataId);
                target.Set(rows);
                return;
            }
            if (_command == AB_DB_ChatTurn_Command_Type.INFO_DELETE_BY_PACKAGE)
            {
                long pkg_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey).Get();
                List<AB_Object_DB_Chat_Turn> rows = _db.FindAsync_<AB_Object_DB_Chat_Turn>(_txn, _x => _x.PackageId == pkg_id).GetAwaiter().GetResult();
                foreach (AB_Object_DB_Chat_Turn row in rows)
                {
                    _db.RemoveAsync_(_txn, row).GetAwaiter().GetResult();
                }
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleChatTurnDb: 미등록 Command=" + _command);
        }

        private void HandleLogicDb(AB_Object_DB _db, EDP_Db_Transaction _txn, AB_DB_Logic_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            if (m_blackboard == null) throw new InvalidOperationException("AB_Manager_DB.HandleLogicDb: Blackboard 미부착");
            if (_command == AB_DB_Logic_Command_Type.None) throw new InvalidOperationException("AB_Manager_DB.HandleLogicDb: None 위반");
            if (_command == AB_DB_Logic_Command_Type.End) throw new InvalidOperationException("AB_Manager_DB.HandleLogicDb: End 위반");
            if (_command == AB_DB_Logic_Command_Type.INFO_GET_ALL)
            {
                List<AB_Object_DB_Logic> all = _db.FindAsync_<AB_Object_DB_Logic>(_txn, _x => true).GetAwaiter().GetResult();
                AB_Data_DB_Logic_List target = m_blackboard.Lookup<AB_Data_DB_Logic_List>(_env.TargetDataId);
                target.Set(all);
                return;
            }
            if (_command == AB_DB_Logic_Command_Type.INFO_GET)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                AB_Object_DB_Logic? entity = _db.GetByIdAsync_<AB_Object_DB_Logic>(_txn, input_id.Get()).GetAwaiter().GetResult();
                AB_Data_DB_Logic target = m_blackboard.Lookup<AB_Data_DB_Logic>(_env.TargetDataId);
                target.Set(entity);
                return;
            }
            if (_command == AB_DB_Logic_Command_Type.INFO_ADD)
            {
                AB_Object_DB_Logic? entity = m_blackboard.Lookup<AB_Data_DB_Logic>(_env.DataKey).Get();
                if (entity == null) throw new InvalidOperationException("AB_Manager_DB.HandleLogicDb.INFO_ADD: entity null");
                _db.AddRowAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Logic_Command_Type.INFO_SAVE)
            {
                AB_Object_DB_Logic? entity = m_blackboard.Lookup<AB_Data_DB_Logic>(_env.DataKey).Get();
                if (entity == null) throw new InvalidOperationException("AB_Manager_DB.HandleLogicDb.INFO_SAVE: entity null");
                _db.UpdateAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Logic_Command_Type.INFO_DELETE)
            {
                long id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey).Get();
                AB_Object_DB_Logic? entity = _db.GetByIdAsync_<AB_Object_DB_Logic>(_txn, id).GetAwaiter().GetResult();
                if (entity == null) throw new InvalidOperationException("AB_Manager_DB.HandleLogicDb.INFO_DELETE: id 미존재=" + id);
                _db.RemoveAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleLogicDb: 미등록 Command=" + _command);
        }

        private void HandleModelDb(AB_Object_DB _db, EDP_Db_Transaction _txn, AB_DB_Model_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            if (m_blackboard == null) throw new InvalidOperationException("AB_Manager_DB.HandleModelDb: Blackboard 미부착");
            if (_command == AB_DB_Model_Command_Type.None) throw new InvalidOperationException("AB_Manager_DB.HandleModelDb: None 위반");
            if (_command == AB_DB_Model_Command_Type.End) throw new InvalidOperationException("AB_Manager_DB.HandleModelDb: End 위반");
            if (_command == AB_DB_Model_Command_Type.INFO_GET_ALL)
            {
                List<AB_Object_DB_Model> all = _db.FindAsync_<AB_Object_DB_Model>(_txn, _x => true).GetAwaiter().GetResult();
                AB_Data_DB_Model_List target = m_blackboard.Lookup<AB_Data_DB_Model_List>(_env.TargetDataId);
                target.Set(all);
                return;
            }
            if (_command == AB_DB_Model_Command_Type.INFO_GET)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                AB_Object_DB_Model? entity = _db.GetByIdAsync_<AB_Object_DB_Model>(_txn, input_id.Get()).GetAwaiter().GetResult();
                AB_Data_DB_Model target = m_blackboard.Lookup<AB_Data_DB_Model>(_env.TargetDataId);
                target.Set(entity);
                return;
            }
            if (_command == AB_DB_Model_Command_Type.INFO_ADD)
            {
                AB_Object_DB_Model? entity = m_blackboard.Lookup<AB_Data_DB_Model>(_env.DataKey).Get();
                if (entity == null) throw new InvalidOperationException("AB_Manager_DB.HandleModelDb.INFO_ADD: entity null");
                _db.AddRowAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Model_Command_Type.INFO_SAVE)
            {
                AB_Object_DB_Model? entity = m_blackboard.Lookup<AB_Data_DB_Model>(_env.DataKey).Get();
                if (entity == null) throw new InvalidOperationException("AB_Manager_DB.HandleModelDb.INFO_SAVE: entity null");
                _db.UpdateAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Model_Command_Type.INFO_DELETE)
            {
                long id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey).Get();
                AB_Object_DB_Model? entity = _db.GetByIdAsync_<AB_Object_DB_Model>(_txn, id).GetAwaiter().GetResult();
                if (entity == null) throw new InvalidOperationException("AB_Manager_DB.HandleModelDb.INFO_DELETE: id 미존재=" + id);
                _db.RemoveAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleModelDb: 미등록 Command=" + _command);
        }

        // === Turn 도메인 핸들러 (round 3) ===
        //   slot 타입:
        //     CREATE_BUCKET        DataKey = AB_Data_DB_Turn_Row (caller 매개 entity 사전 채움 — TurnId / PrevTurnId / CreatedAt / ResultCount=0 / SelectedIndex=0). TargetDataId = 0
        //     APPEND_RESULT        DataKey = AB_Data_Long (TurnId). 매니저 매개 GetByIdAsync_ → ResultCount += 1 → UpdateAsync_. SelectedIndex 미 변경 (W2=b)
        //     SET_SELECTED_INDEX   DataKey = AB_Data_DB_Turn_Row (caller 매개 TurnId + SelectedIndex 채움). 매니저 매개 GetByIdAsync_ → existing.SelectedIndex = input.SelectedIndex → UpdateAsync_
        //     GET_BY_ID            DataKey = AB_Data_Long (TurnId). TargetDataId = AB_Data_DB_Turn_Row
        //     FIND_RANGE           DataKey = AB_Data_Long (turn_id anchor — 본 round 미 사용, 단일 shard 전수 반환). TargetDataId = AB_Data_DB_Turn_List
        private void HandleTurnDb(AB_Object_DB _db, EDP_Db_Transaction _txn, AB_DB_Turn_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            if (m_blackboard == null)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleTurnDb: Blackboard 미부착 — AttachBlackboard 선 호출");
            }
            if (_command == AB_DB_Turn_Command_Type.None)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleTurnDb: None Command 위반");
            }
            if (_command == AB_DB_Turn_Command_Type.End)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleTurnDb: End Command 위반");
            }
            if (_command == AB_DB_Turn_Command_Type.CREATE_BUCKET)
            {
                AB_Data_DB_Turn_Row input = m_blackboard.Lookup<AB_Data_DB_Turn_Row>(_env.DataKey);
                AB_Object_DB_Turn_Row? entity = input.Get();
                if (entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleTurnDb.CREATE_BUCKET: 입력 entity null — DataKey=" + _env.DataKey);
                }
                _db.AddRowAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Turn_Command_Type.APPEND_RESULT)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                long turn_id = input_id.Get();
                AB_Object_DB_Turn_Row? existing = _db.GetByIdAsync_<AB_Object_DB_Turn_Row>(_txn, turn_id).GetAwaiter().GetResult();
                if (existing == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleTurnDb.APPEND_RESULT: 대상 turn row 미존재 TurnId=" + turn_id);
                }
                existing.ResultCount += 1;
                _db.UpdateAsync_(_txn, existing).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Turn_Command_Type.SET_SELECTED_INDEX)
            {
                AB_Data_DB_Turn_Row input = m_blackboard.Lookup<AB_Data_DB_Turn_Row>(_env.DataKey);
                AB_Object_DB_Turn_Row? input_entity = input.Get();
                if (input_entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleTurnDb.SET_SELECTED_INDEX: 입력 entity null — DataKey=" + _env.DataKey);
                }
                AB_Object_DB_Turn_Row? existing = _db.GetByIdAsync_<AB_Object_DB_Turn_Row>(_txn, input_entity.TurnId).GetAwaiter().GetResult();
                if (existing == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleTurnDb.SET_SELECTED_INDEX: 대상 turn row 미존재 TurnId=" + input_entity.TurnId);
                }
                existing.SelectedIndex = input_entity.SelectedIndex;
                _db.UpdateAsync_(_txn, existing).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Turn_Command_Type.GET_BY_ID)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                long turn_id = input_id.Get();
                AB_Object_DB_Turn_Row? entity = _db.GetByIdAsync_<AB_Object_DB_Turn_Row>(_txn, turn_id).GetAwaiter().GetResult();
                AB_Data_DB_Turn_Row target = m_blackboard.Lookup<AB_Data_DB_Turn_Row>(_env.TargetDataId);
                target.Set(entity);
                return;
            }
            if (_command == AB_DB_Turn_Command_Type.FIND_RANGE)
            {
                // round 3 = 단일 shard 안 전수 반환 (W3 = cross-shard sweep 별도 round 5 매개). 본 round = caller 매개 shard 선택 후 그 shard 안 전수.
                List<AB_Object_DB_Turn_Row> all = _db.FindAsync_<AB_Object_DB_Turn_Row>(_txn, _x => true).GetAwaiter().GetResult();
                AB_Data_DB_Turn_List target = m_blackboard.Lookup<AB_Data_DB_Turn_List>(_env.TargetDataId);
                target.Set(all);
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleTurnDb: 미등록 Command=" + _command);
        }

        // === Result 도메인 핸들러 (round 3) ===
        //   slot 타입:
        //     PUT           DataKey = AB_Data_DB_Node_Row (caller 매개 entity 사전 채움 — DataKey / TurnId / ResultSeq / NodeNumbers / Payload / CreatedAt). TargetDataId = 0
        //     GET_BY_KEY    DataKey = AB_Data_Long (data_key). TargetDataId = AB_Data_DB_Node_Row
        //     FIND_BY_TURN  DataKey = AB_Data_Long (turn_id). TargetDataId = AB_Data_DB_Node_List. 단일 shard 안 WHERE TurnId == turn_id
        //     FIND_BY_NODE  DataKey = AB_Data_Long (미 사용 round 3). TargetDataId = AB_Data_DB_Node_List. 단일 shard 전수 반환 (W3 round 5 매개 cross-shard sweep)
        private void HandleResultDb(AB_Object_DB _db, EDP_Db_Transaction _txn, AB_DB_Result_Command_Type _command, AB_Object_DB_Request_Envelope _env)
        {
            if (m_blackboard == null)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleResultDb: Blackboard 미부착 — AttachBlackboard 선 호출");
            }
            if (_command == AB_DB_Result_Command_Type.None)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleResultDb: None Command 위반");
            }
            if (_command == AB_DB_Result_Command_Type.End)
            {
                throw new InvalidOperationException("AB_Manager_DB.HandleResultDb: End Command 위반");
            }
            if (_command == AB_DB_Result_Command_Type.PUT)
            {
                AB_Data_DB_Node_Row input = m_blackboard.Lookup<AB_Data_DB_Node_Row>(_env.DataKey);
                AB_Object_DB_Node_Row? entity = input.Get();
                if (entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleResultDb.PUT: 입력 entity null — DataKey=" + _env.DataKey);
                }
                _db.AddRowAsync_(_txn, entity).GetAwaiter().GetResult();
                return;
            }
            if (_command == AB_DB_Result_Command_Type.GET_BY_KEY)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                long data_key = input_id.Get();
                AB_Object_DB_Node_Row? entity = _db.GetByIdAsync_<AB_Object_DB_Node_Row>(_txn, data_key).GetAwaiter().GetResult();
                AB_Data_DB_Node_Row target = m_blackboard.Lookup<AB_Data_DB_Node_Row>(_env.TargetDataId);
                target.Set(entity);
                return;
            }
            if (_command == AB_DB_Result_Command_Type.FIND_BY_TURN)
            {
                AB_Data_Long input_id = m_blackboard.Lookup<AB_Data_Long>(_env.DataKey);
                long turn_id = input_id.Get();
                List<AB_Object_DB_Node_Row> all = _db.FindAsync_<AB_Object_DB_Node_Row>(_txn, _x => _x.TurnId == turn_id).GetAwaiter().GetResult();
                AB_Data_DB_Node_List target = m_blackboard.Lookup<AB_Data_DB_Node_List>(_env.TargetDataId);
                target.Set(all);
                return;
            }
            if (_command == AB_DB_Result_Command_Type.FIND_BY_NODE)
            {
                // round 3 = 단일 shard 안 전수 반환 (cross-shard sweep = round 5 lazy).
                List<AB_Object_DB_Node_Row> all = _db.FindAsync_<AB_Object_DB_Node_Row>(_txn, _x => true).GetAwaiter().GetResult();
                AB_Data_DB_Node_List target = m_blackboard.Lookup<AB_Data_DB_Node_List>(_env.TargetDataId);
                target.Set(all);
                return;
            }
            if (_command == AB_DB_Result_Command_Type.FIND_SIMILAR)
            {
                // 13r4b 매개 — 입력 AB_Data_DB_Similarity_Query (QueryVector + TopK) / 출력 AB_Data_DB_Node_List.
                // 단일 shard 안 sqlite-vec vec_distance_cosine 매개 active row 매개 ORDER BY + LIMIT TopK.
                AB_Data_DB_Similarity_Query input = m_blackboard.Lookup<AB_Data_DB_Similarity_Query>(_env.DataKey);
                AB_Object_DB_Similarity_Query? query = input.Get();
                if (query == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleResultDb.FIND_SIMILAR: 입력 query null — DataKey=" + _env.DataKey);
                }
                if (query.QueryVector == null || query.QueryVector.Length == 0)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleResultDb.FIND_SIMILAR: QueryVector null/0 — DataKey=" + _env.DataKey);
                }
                if (query.TopK <= 0)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleResultDb.FIND_SIMILAR: TopK <= 0 (=" + query.TopK + ") — DataKey=" + _env.DataKey);
                }
                byte[] query_bytes = VectorToBytes_(query.QueryVector);
                string sql = "SELECT * FROM \"NodeRows\" WHERE \"IsActive\" = 1 AND \"Embedding\" IS NOT NULL ORDER BY vec_distance_cosine(\"Embedding\", {0}) ASC LIMIT {1}";
                List<AB_Object_DB_Node_Row> hits = _db.FindSimilarAsync_<AB_Object_DB_Node_Row>(_txn, sql, query_bytes, query.TopK).GetAwaiter().GetResult();
                AB_Data_DB_Node_List target = m_blackboard.Lookup<AB_Data_DB_Node_List>(_env.TargetDataId);
                target.Set(hits);
                return;
            }
            if (_command == AB_DB_Result_Command_Type.SET_IS_ACTIVE)
            {
                // 13r4b 매개 — 입력 AB_Data_DB_Node_Row (DataKey + IsActive 채움). 매니저 매개 GetByIdAsync_ → existing.IsActive = input.IsActive → UpdateAsync_.
                AB_Data_DB_Node_Row input = m_blackboard.Lookup<AB_Data_DB_Node_Row>(_env.DataKey);
                AB_Object_DB_Node_Row? input_entity = input.Get();
                if (input_entity == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleResultDb.SET_IS_ACTIVE: 입력 entity null — DataKey=" + _env.DataKey);
                }
                AB_Object_DB_Node_Row? existing = _db.GetByIdAsync_<AB_Object_DB_Node_Row>(_txn, input_entity.DataKey).GetAwaiter().GetResult();
                if (existing == null)
                {
                    throw new InvalidOperationException("AB_Manager_DB.HandleResultDb.SET_IS_ACTIVE: 대상 row 미존재 DataKey=" + input_entity.DataKey);
                }
                existing.IsActive = input_entity.IsActive;
                _db.UpdateAsync_(_txn, existing).GetAwaiter().GetResult();
                return;
            }
            throw new InvalidOperationException("AB_Manager_DB.HandleResultDb: 미등록 Command=" + _command);
        }

        // 13r4b 매개 float[] → byte[] little-endian 매개 변환. sqlite-vec vec0 매개 BLOB 매개 little-endian float32 매개 가정.
        // 본 helper = handler 매개 매 FIND_SIMILAR 호출 시점 매개 vector → bytes 매개. 캐싱 매개 caller 책임 (반복 호출 매개 매번 변환).
        private static byte[] VectorToBytes_(float[] _vector)
        {
            byte[] bytes = new byte[_vector.Length * sizeof(float)];
            System.Buffer.BlockCopy(_vector, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public void SyncDirtyToFile()
        {
            m_engine.SyncDirtyToFile();
        }

        public override void Tick(double _delta_sec)
        {
            base.Tick(_delta_sec);
            DrainQueue();
            SyncDirtyToFile();
        }

        public override void Dispose()
        {
            foreach (AB_Object_DB db in m_db_objects.Values)
            {
                db.Dispose();
            }
            m_db_objects.Clear();
            m_engine.DisposeAsync().GetAwaiter().GetResult();
            m_pool_manager = null;
            m_blackboard = null;
        }
    }
}
