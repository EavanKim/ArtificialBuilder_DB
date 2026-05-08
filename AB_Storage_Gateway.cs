using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using EDPFW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 4 계층 저장소 ([[storage-layers]]) 게이트웨이. 토픽 db.storage 구독.
    /// Resource / Session / Context / Node 4 storage 의 CRUD 단일 진입점.
    /// 활성 Persona DB 핸들 사용 (모든 4 storage 가 Persona 도메인).
    /// </summary>
    public class AB_Storage_Gateway : ArtificialBuilder_EDP.Core.AB_Component
    {
        private IAB_Message_Broker? m_broker;
        private AB_Subscription_Token? m_sub;
        private EDP_Db_Engine m_engine => AB_Board.Db;

        public override void OnAttach()
        {
            try
            {
                m_broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
                m_sub = m_broker.Subscribe(AB_Storage_Topics.Storage, HandleMessage);
            }
            catch (Exception ex)
            {
                AB_Log.Error("StorageGw", $"OnAttach 실패: {ex.Message}");
            }
        }

        public override void OnDetach()
        {
            try
            {
                if (m_broker != null && m_sub != null)
                    m_broker.Unsubscribe(m_sub);
            }
            catch { }
        }

        private async void HandleMessage(AB_Message _msg)
        {
            if (_msg.IsResponse) return;

            try
            {
                var persona = AB_Board.Persona;
                int handle = persona?.Handle ?? 0;

                switch (_msg)
                {
                    // ============================================================
                    // Resource Storage
                    // ============================================================
                    case AB_Add_Resource_Request req:
                    {
                        long newId = 0;
                        bool ok = false;
                        if (handle != 0)
                        {
                            AB_Resource_Storage_Model row = new()
                            {
                                Kind_ = req.Kind,
                                Size_ = req.Size,
                                PayloadInline_ = req.PayloadInline,
                                PayloadPath_ = req.PayloadPath,
                                CreatedAt_ = DateTime.UtcNow,
                            };
                            await m_engine.AddAsync(handle, row);
                            await m_engine.SaveChangesAsync(handle);
                            newId = row.Id_;
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Resource_Response
                        { CorrelationId = req.CorrelationId, Id = newId, Success = ok });
                        break;
                    }
                    case AB_Get_Resource_Request req:
                    {
                        AB_Resource_Storage_Model? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_Resource_Storage_Model>(handle, _r => _r.Id_ == req.Id && !_r.IsDeleted_);
                            data = found.FirstOrDefault();
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_Resource_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Delete_Resource_Request req:
                    {
                        bool ok = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_Resource_Storage_Model>(handle, _r => _r.Id_ == req.Id);
                            var target = found.FirstOrDefault();
                            if (target != null)
                            {
                                m_engine.Remove(handle, target);
                                await m_engine.SaveChangesAsync(handle);
                            }
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Delete_Resource_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }

                    // ============================================================
                    // Session Storage — turn 슬롯 1 직선 linked list
                    // ============================================================
                    case AB_Append_Session_Slot_Request req:
                    {
                        long newId = 0;
                        bool ok = false;
                        if (handle != 0)
                        {
                            // tail 슬롯 찾기 — !IsDeleted_ 필터 필수.
                            // 마킹된 슬롯이 NextId_=null 인 경우가 있는데 (마킹은 IsDeleted_ 만 켜고 포인터는 안 건드림),
                            // 그걸 tail 로 잡으면 새 슬롯이 marked 슬롯의 PrevId 를 가리킴 → sweep 시점에
                            // marked 슬롯이 "live ref 있다" 로 revive 됨 → 삭제했던 turn 이 부활.
                            var tail = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle,
                                _s => _s.SessionId_ == req.SessionId && _s.NextId_ == null && !_s.IsDeleted_)).FirstOrDefault();

                            AB_Session_Storage_Model row = new()
                            {
                                SessionId_ = req.SessionId,
                                PrevId_ = tail?.Id_,
                                NextId_ = null,
                                InputResourceId_ = req.InputResourceId,
                                ActiveContextId_ = null,
                                CreatedAt_ = DateTime.UtcNow,
                            };
                            await m_engine.AddAsync(handle, row);
                            await m_engine.SaveChangesAsync(handle);
                            newId = row.Id_;

                            // 살아있는 tail 의 next_id 만 갱신 (marked tail 은 어차피 sweep 대상이라 무관).
                            if (tail != null)
                            {
                                tail.NextId_ = newId;
                                m_engine.Update(handle, tail);
                                await m_engine.SaveChangesAsync(handle);
                            }
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Append_Session_Slot_Response
                        { CorrelationId = req.CorrelationId, Id = newId, Success = ok });
                        break;
                    }
                    case AB_Get_Session_Slot_Request req:
                    {
                        AB_Session_Storage_Model? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.Id_ == req.Id && !_s.IsDeleted_);
                            data = found.FirstOrDefault();
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_Session_Slot_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Get_Session_Slots_Request req:
                    {
                        List<AB_Session_Storage_Model> data = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.SessionId_ == req.SessionId && !_s.IsDeleted_);
                            // head 부터 next_id 따라가며 정렬 (마킹된 슬롯은 이미 제외)
                            Dictionary<long, AB_Session_Storage_Model> byId = new();
                            foreach (AB_Session_Storage_Model s in all) byId[s.Id_] = s;
                            AB_Session_Storage_Model? head = null;
                            foreach (AB_Session_Storage_Model s in all)
                            {
                                if (s.PrevId_ == null || (s.PrevId_.HasValue && !byId.ContainsKey(s.PrevId_.Value)))
                                {
                                    head = s;
                                    break;
                                }
                            }
                            while (head != null)
                            {
                                data.Add(head);
                                if (head.NextId_.HasValue && byId.TryGetValue(head.NextId_.Value, out var next))
                                    head = next;
                                else
                                    head = null;
                            }
                        }
                        m_broker?.Publish(new AB_Get_Session_Slots_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Set_Active_Context_Request req:
                    {
                        bool ok = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.Id_ == req.SlotId);
                            var slot = found.FirstOrDefault();
                            if (slot != null)
                            {
                                slot.ActiveContextId_ = req.ActiveContextId;
                                m_engine.Update(handle, slot);
                                await m_engine.SaveChangesAsync(handle);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Set_Active_Context_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Delete_Session_Slot_Request req:
                    {
                        bool ok = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.Id_ == req.Id);
                            var target = found.FirstOrDefault();
                            if (target != null)
                            {
                                // prev / next 재연결
                                if (target.PrevId_.HasValue)
                                {
                                    var prev = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.Id_ == target.PrevId_.Value)).FirstOrDefault();
                                    if (prev != null)
                                    {
                                        prev.NextId_ = target.NextId_;
                                        m_engine.Update(handle, prev);
                                    }
                                }
                                if (target.NextId_.HasValue)
                                {
                                    var next = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.Id_ == target.NextId_.Value)).FirstOrDefault();
                                    if (next != null)
                                    {
                                        next.PrevId_ = target.PrevId_;
                                        m_engine.Update(handle, next);
                                    }
                                }
                                m_engine.Remove(handle, target);
                                await m_engine.SaveChangesAsync(handle);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Delete_Session_Slot_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }

                    // ============================================================
                    // Context Storage — 1 회 실행 단위
                    // ============================================================
                    case AB_Add_Context_Request req:
                    {
                        long newId = 0;
                        bool ok = false;
                        if (handle != 0)
                        {
                            AB_Context_Storage_Model row = new()
                            {
                                SessionId_ = req.SessionId,
                                TurnId_ = req.TurnId,
                                CreatedAt_ = DateTime.UtcNow,
                            };
                            await m_engine.AddAsync(handle, row);
                            await m_engine.SaveChangesAsync(handle);
                            newId = row.Id_;
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Context_Response
                        { CorrelationId = req.CorrelationId, Id = newId, Success = ok });
                        break;
                    }
                    case AB_Get_Contexts_By_Turn_Request req:
                    {
                        List<AB_Context_Storage_Model> data = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.FindAsync<AB_Context_Storage_Model>(handle, _c => _c.TurnId_ == req.TurnId && !_c.IsDeleted_);
                            int CompareCreated(AB_Context_Storage_Model _a, AB_Context_Storage_Model _b) => _a.CreatedAt_.CompareTo(_b.CreatedAt_);
                            data = new List<AB_Context_Storage_Model>(all);
                            data.Sort(CompareCreated);
                        }
                        m_broker?.Publish(new AB_Get_Contexts_By_Turn_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Delete_Context_Request req:
                    {
                        bool ok = false;
                        if (handle != 0)
                        {
                            // 노드 cascade 제거
                            var nodes = await m_engine.FindAsync<AB_Logic_Storage_Model>(handle, _n => _n.ContextId_ == req.Id);
                            foreach (var n in nodes)
                                m_engine.Remove(handle, n);
                            var found = await m_engine.FindAsync<AB_Context_Storage_Model>(handle, _c => _c.Id_ == req.Id);
                            var target = found.FirstOrDefault();
                            if (target != null) m_engine.Remove(handle, target);
                            await m_engine.SaveChangesAsync(handle);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Delete_Context_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }

                    // ============================================================
                    // Node Storage — 1 노드 1 회 실행
                    // ============================================================
                    case AB_Add_Node_Request req:
                    {
                        long newId = 0;
                        bool ok = false;
                        if (handle != 0)
                        {
                            AB_Logic_Storage_Model row = new()
                            {
                                ContextId_ = req.ContextId,
                                NodeId_ = req.NodeId,
                                EmissionOrder_ = req.EmissionOrder,
                                ResourceId_ = req.ResourceId,
                                MetaJson_ = req.MetaJson,
                                CreatedAt_ = DateTime.UtcNow,
                            };
                            await m_engine.AddAsync(handle, row);
                            await m_engine.SaveChangesAsync(handle);
                            newId = row.Id_;
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Node_Response
                        { CorrelationId = req.CorrelationId, Id = newId, Success = ok });
                        break;
                    }
                    case AB_Get_Nodes_By_Context_Request req:
                    {
                        List<AB_Logic_Storage_Model> data = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.FindAsync<AB_Logic_Storage_Model>(handle, _n => _n.ContextId_ == req.ContextId && !_n.IsDeleted_);
                            int CompareEmission(AB_Logic_Storage_Model _a, AB_Logic_Storage_Model _b) => _a.EmissionOrder_.CompareTo(_b.EmissionOrder_);
                            data = new List<AB_Logic_Storage_Model>(all);
                            data.Sort(CompareEmission);
                        }
                        m_broker?.Publish(new AB_Get_Nodes_By_Context_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Delete_Node_Request req:
                    {
                        // 단건 row 삭제 — resource 는 orphan 그대로 (별도 정리 책임).
                        // 단순 분리 정책 — cascade 안 하므로 빠르고 read 경로는 즉시 해당 노드 안 보임.
                        bool ok = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_Logic_Storage_Model>(handle, _n => _n.Id_ == req.Id);
                            var target = found.FirstOrDefault();
                            if (target != null)
                            {
                                m_engine.Remove(handle, target);
                                await m_engine.SaveChangesAsync(handle);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Delete_Node_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }

                    // ============================================================
                    // Pending deletion 큐 — leaf-first time-budgeted sweep
                    // ============================================================
                    case AB_Enqueue_Deletion_Request req:
                    {
                        // Mark phase: pending row + subtree 전체 IsDeleted_=true cascade.
                        // 한 SaveChangesAsync 안에 atomic — 중간 크래시 방지.
                        // sweep 은 다음 틱부터 IsDeleted_=true row 만 보면 됨 (재귀 검사 불필요).
                        bool ok = false;
                        if (handle != 0)
                        {
                            var existing = await m_engine.FindAsync<AB_Pending_Deletion_Storage_Model>(handle,
                                _p => _p.TargetTable_ == req.TargetTable && _p.TargetId_ == req.TargetId);
                            if (!existing.Any())
                            {
                                var row = new AB_Pending_Deletion_Storage_Model
                                {
                                    TargetTable_ = req.TargetTable,
                                    TargetId_ = req.TargetId,
                                    EnqueuedAt_ = DateTime.UtcNow,
                                };
                                await m_engine.AddAsync(handle, row);

                                if (req.TargetTable == "context_storage")
                                {
                                    // ctx + 하위 모든 nodes mark.
                                    var ctx = (await m_engine.FindAsync<AB_Context_Storage_Model>(handle, _c => _c.Id_ == req.TargetId)).FirstOrDefault();
                                    if (ctx != null && !ctx.IsDeleted_)
                                    {
                                        ctx.IsDeleted_ = true;
                                        m_engine.Update(handle, ctx);
                                    }
                                    var nodes = (await m_engine.FindAsync<AB_Logic_Storage_Model>(handle, _n => _n.ContextId_ == req.TargetId && !_n.IsDeleted_)).ToList();
                                    foreach (var n in nodes)
                                    {
                                        n.IsDeleted_ = true;
                                        m_engine.Update(handle, n);
                                    }
                                }
                                else if (req.TargetTable == "session_storage")
                                {
                                    // slot + 하위 모든 contexts + 그 하위 nodes mark.
                                    var slot = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.Id_ == req.TargetId)).FirstOrDefault();
                                    if (slot != null && !slot.IsDeleted_)
                                    {
                                        slot.IsDeleted_ = true;
                                        m_engine.Update(handle, slot);
                                    }
                                    var ctxs = (await m_engine.FindAsync<AB_Context_Storage_Model>(handle, _c => _c.TurnId_ == req.TargetId && !_c.IsDeleted_)).ToList();
                                    HashSet<long> ctxIds = new();
                                    foreach (var c in ctxs)
                                    {
                                        c.IsDeleted_ = true;
                                        m_engine.Update(handle, c);
                                        ctxIds.Add(c.Id_);
                                    }
                                    if (ctxIds.Count > 0)
                                    {
                                        var nodes = (await m_engine.FindAsync<AB_Logic_Storage_Model>(handle, _n => ctxIds.Contains(_n.ContextId_) && !_n.IsDeleted_)).ToList();
                                        foreach (var n in nodes)
                                        {
                                            n.IsDeleted_ = true;
                                            m_engine.Update(handle, n);
                                        }
                                    }
                                }

                                await m_engine.SaveChangesAsync(handle);
                            }
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Enqueue_Deletion_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Sweep_Deletion_Step_Request req:
                    {
                        // mark-sweep 패턴 — 마킹은 enqueue 시점에 cascade 로 끝났으므로
                        // sweep 은 IsDeleted_=true row 만 leaf-first 로 실제 DELETE.
                        // ref check 는 !IsDeleted_ 만 카운트 → 살아있는 ref 만 본다 (drift 없는 live count).
                        int rowsDeleted = 0;
                        bool drained = true;
                        if (handle != 0)
                        {
                            var sw = Stopwatch.StartNew();
                            long budgetTicks = (req.BudgetMicros * Stopwatch.Frequency) / 1_000_000L;
                            int CompareEnqueued(AB_Pending_Deletion_Storage_Model _a, AB_Pending_Deletion_Storage_Model _b) => _a.EnqueuedAt_.CompareTo(_b.EnqueuedAt_);
                            List<AB_Pending_Deletion_Storage_Model> pendingAll = new(await m_engine.FindAsync<AB_Pending_Deletion_Storage_Model>(handle, _p => true));
                            pendingAll.Sort(CompareEnqueued);
                            if (pendingAll.Count == 0) drained = true;
                            else drained = false;

                            foreach (var pending in pendingAll)
                            {
                                if (sw.ElapsedTicks > budgetTicks) { drained = false; break; }

                                if (pending.TargetTable_ == "context_storage")
                                {
                                    // 마킹된 노드 batch 처리. node 는 inbound ref 없음 → 항상 DELETE.
                                    // resource 는 leaf — live ref 있으면 leave alone, 없으면 DELETE.
                                    var nodes = (await m_engine.FindAsync<AB_Logic_Storage_Model>(handle,
                                        _n => _n.ContextId_ == pending.TargetId_ && _n.IsDeleted_))
                                        .Take(8).ToList();
                                    if (nodes.Count > 0)
                                    {
                                        foreach (var n in nodes)
                                        {
                                            if (sw.ElapsedTicks > budgetTicks) break;
                                            if (n.ResourceId_.HasValue)
                                            {
                                                long rid = n.ResourceId_.Value;
                                                // live ref count — !IsDeleted_ 만. 마킹된 자손은 자동 제외 (drift 없는 live count).
                                                int liveNodeRefs = (await m_engine.FindAsync<AB_Logic_Storage_Model>(handle,
                                                    _x => _x.ResourceId_ == rid && _x.Id_ != n.Id_ && !_x.IsDeleted_)).Count();
                                                int liveSlotRefs = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle,
                                                    _s => _s.InputResourceId_ == rid && !_s.IsDeleted_)).Count();
                                                if (liveNodeRefs == 0 && liveSlotRefs == 0)
                                                {
                                                    // 4-2: 살아있는 ref 없음 → DELETE.
                                                    var res = (await m_engine.FindAsync<AB_Resource_Storage_Model>(handle, _r => _r.Id_ == rid)).FirstOrDefault();
                                                    if (res != null) { m_engine.Remove(handle, res); rowsDeleted++; }
                                                }
                                                // 4-1: 살아있는 ref 있음 → resource 그대로 (마킹 안 했으므로 revive 불필요).
                                            }
                                            m_engine.Remove(handle, n);
                                            rowsDeleted++;
                                        }
                                        await m_engine.SaveChangesAsync(handle);
                                    }
                                    else
                                    {
                                        // 자식 노드 모두 정리 끝 → context 자체 삭제 검증.
                                        var ctx = (await m_engine.FindAsync<AB_Context_Storage_Model>(handle, _c => _c.Id_ == pending.TargetId_)).FirstOrDefault();
                                        if (ctx != null)
                                        {
                                            // 4: !IsDeleted slot 이 이 context 를 active 로 가리키는지 확인.
                                            int liveSlotRefs = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle,
                                                _s => _s.ActiveContextId_ == ctx.Id_ && !_s.IsDeleted_)).Count();
                                            if (liveSlotRefs > 0)
                                            {
                                                // 4-1: 살아있는 ref 있음 → revive. 사용자가 의도와 달리 active 옮겨놓지 않은 상태.
                                                ctx.IsDeleted_ = false;
                                                m_engine.Update(handle, ctx);
                                                AB_Log.Warn("StorageGw", $"Sweep revive — ctx={ctx.Id_} 가 live slot 의 ActiveContext (mark cascade 시점에 active 미이동). IsDeleted=false 복구");
                                            }
                                            else
                                            {
                                                // 4-2: 살아있는 ref 없음 → DELETE.
                                                m_engine.Remove(handle, ctx);
                                                rowsDeleted++;
                                            }
                                        }
                                        m_engine.Remove(handle, pending);
                                        await m_engine.SaveChangesAsync(handle);
                                    }
                                }
                                else if (pending.TargetTable_ == "session_storage")
                                {
                                    // 마킹된 자식 contexts 직접 처리 (마킹 이미 됐으니 enqueue 안 해도 sweep 해줌).
                                    // 자식 ctx 의 자식 nodes 한 batch 처리. 모두 처리되면 ctx 도 삭제.
                                    var markedCtxs = (await m_engine.FindAsync<AB_Context_Storage_Model>(handle,
                                        _c => _c.TurnId_ == pending.TargetId_ && _c.IsDeleted_))
                                        .ToList();
                                    if (markedCtxs.Count > 0)
                                    {
                                        bool anyProcessed = false;
                                        foreach (var c in markedCtxs)
                                        {
                                            if (sw.ElapsedTicks > budgetTicks) break;
                                            var ctxNodes = (await m_engine.FindAsync<AB_Logic_Storage_Model>(handle,
                                                _n => _n.ContextId_ == c.Id_ && _n.IsDeleted_))
                                                .Take(8).ToList();
                                            if (ctxNodes.Count > 0)
                                            {
                                                foreach (var n in ctxNodes)
                                                {
                                                    if (sw.ElapsedTicks > budgetTicks) break;
                                                    if (n.ResourceId_.HasValue)
                                                    {
                                                        long rid = n.ResourceId_.Value;
                                                        int liveNodeRefs = (await m_engine.FindAsync<AB_Logic_Storage_Model>(handle,
                                                            _x => _x.ResourceId_ == rid && _x.Id_ != n.Id_ && !_x.IsDeleted_)).Count();
                                                        int liveSlotRefs = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle,
                                                            _s => _s.InputResourceId_ == rid && !_s.IsDeleted_)).Count();
                                                        if (liveNodeRefs == 0 && liveSlotRefs == 0)
                                                        {
                                                            var res = (await m_engine.FindAsync<AB_Resource_Storage_Model>(handle, _r => _r.Id_ == rid)).FirstOrDefault();
                                                            if (res != null) { m_engine.Remove(handle, res); rowsDeleted++; }
                                                        }
                                                    }
                                                    m_engine.Remove(handle, n);
                                                    rowsDeleted++;
                                                }
                                                anyProcessed = true;
                                            }
                                            else
                                            {
                                                // 이 ctx 의 nodes 모두 정리 → ctx 자체 삭제.
                                                m_engine.Remove(handle, c);
                                                rowsDeleted++;
                                                anyProcessed = true;
                                            }
                                        }
                                        if (anyProcessed) await m_engine.SaveChangesAsync(handle);
                                    }
                                    else
                                    {
                                        // 자식 ctx 모두 정리 끝 → 슬롯 자체 삭제 검증.
                                        var slot = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.Id_ == pending.TargetId_)).FirstOrDefault();
                                        if (slot != null)
                                        {
                                            // 4: !IsDeleted slot 이 이 슬롯을 prev/next 로 참조하는지 확인.
                                            int livePrevRefs = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle,
                                                _s => _s.PrevId_ == slot.Id_ && !_s.IsDeleted_)).Count();
                                            int liveNextRefs = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle,
                                                _s => _s.NextId_ == slot.Id_ && !_s.IsDeleted_)).Count();
                                            // input_resource_id 는 leaf 처리 (해당 resource 의 ref 카운트에 슬롯 자기 자신 빼고 봄) — 슬롯 자체 revive 와 무관.
                                            if (livePrevRefs > 0 || liveNextRefs > 0)
                                            {
                                                // 4-1: 살아있는 linked-list 이웃 ref 있음 → revive.
                                                slot.IsDeleted_ = false;
                                                m_engine.Update(handle, slot);
                                                AB_Log.Warn("StorageGw", $"Sweep revive — slot={slot.Id_} 가 live 이웃 슬롯의 prev/next (포인터 정리 누락). IsDeleted=false 복구");
                                            }
                                            else
                                            {
                                                // 4-2: 살아있는 ref 없음 → 이웃 우회 + DELETE.
                                                if (slot.PrevId_.HasValue)
                                                {
                                                    var prev = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.Id_ == slot.PrevId_.Value)).FirstOrDefault();
                                                    if (prev != null && prev.NextId_ == slot.Id_)
                                                    {
                                                        prev.NextId_ = slot.NextId_;
                                                        m_engine.Update(handle, prev);
                                                    }
                                                }
                                                if (slot.NextId_.HasValue)
                                                {
                                                    var nx = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.Id_ == slot.NextId_.Value)).FirstOrDefault();
                                                    if (nx != null && nx.PrevId_ == slot.Id_)
                                                    {
                                                        nx.PrevId_ = slot.PrevId_;
                                                        m_engine.Update(handle, nx);
                                                    }
                                                }
                                                // 슬롯의 input_resource_id 도 leaf 검증 — 다른 살아있는 노드 / 슬롯이 안 쓰면 DELETE.
                                                if (slot.InputResourceId_.HasValue)
                                                {
                                                    long rid = slot.InputResourceId_.Value;
                                                    int liveNodeRefs = (await m_engine.FindAsync<AB_Logic_Storage_Model>(handle,
                                                        _x => _x.ResourceId_ == rid && !_x.IsDeleted_)).Count();
                                                    int liveSlotRefs = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle,
                                                        _s => _s.InputResourceId_ == rid && _s.Id_ != slot.Id_ && !_s.IsDeleted_)).Count();
                                                    if (liveNodeRefs == 0 && liveSlotRefs == 0)
                                                    {
                                                        var res = (await m_engine.FindAsync<AB_Resource_Storage_Model>(handle, _r => _r.Id_ == rid)).FirstOrDefault();
                                                        if (res != null) { m_engine.Remove(handle, res); rowsDeleted++; }
                                                    }
                                                }
                                                m_engine.Remove(handle, slot);
                                                rowsDeleted++;
                                            }
                                        }
                                        m_engine.Remove(handle, pending);
                                        await m_engine.SaveChangesAsync(handle);
                                    }
                                }
                                else
                                {
                                    AB_Log.Warn("StorageGw", $"Sweep: 알 수 없는 target_table={pending.TargetTable_} id={pending.TargetId_}");
                                    m_engine.Remove(handle, pending);
                                    await m_engine.SaveChangesAsync(handle);
                                }
                            }
                        }
                        m_broker?.Publish(new AB_Sweep_Deletion_Step_Response
                        { CorrelationId = req.CorrelationId, RowsDeleted = rowsDeleted, QueueDrained = drained });
                        break;
                    }

                    // ============================================================
                    // 세션 cascade 삭제
                    // ============================================================
                    case AB_Delete_Session_Storage_Request req:
                    {
                        int slotsRemoved = 0, ctxRemoved = 0, nodesRemoved = 0, resRemoved = 0;
                        bool ok = false;
                        if (handle != 0)
                        {
                            // 1) 세션의 슬롯 / 컨텍스트 / 노드 수집.
                            var slots = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.SessionId_ == req.SessionId)).ToList();
                            var contexts = (await m_engine.FindAsync<AB_Context_Storage_Model>(handle, _c => _c.SessionId_ == req.SessionId)).ToList();
                            HashSet<long> ctxIds = new();
                            foreach (AB_Context_Storage_Model c in contexts) ctxIds.Add(c.Id_);
                            var nodes = (await m_engine.FindAsync<AB_Logic_Storage_Model>(handle, _n => ctxIds.Contains(_n.ContextId_))).ToList();

                            // 2) 참조 resource id 수집 — 슬롯의 input + 노드의 output.
                            HashSet<long> resourceIds = new();
                            foreach (var s in slots)
                                if (s.InputResourceId_.HasValue) resourceIds.Add(s.InputResourceId_.Value);
                            foreach (var n in nodes)
                                if (n.ResourceId_.HasValue) resourceIds.Add(n.ResourceId_.Value);

                            // 3) cascade delete — nodes → contexts → slots → resources.
                            foreach (var n in nodes) m_engine.Remove(handle, n);
                            nodesRemoved = nodes.Count;
                            foreach (var c in contexts) m_engine.Remove(handle, c);
                            ctxRemoved = contexts.Count;
                            foreach (var s in slots) m_engine.Remove(handle, s);
                            slotsRemoved = slots.Count;
                            await m_engine.SaveChangesAsync(handle);

                            // 4) resource 는 다른 세션 / 슬롯이 참조 중일 수 있으니 검증 후 삭제 — 이번 세션 외 참조 없으면 제거.
                            foreach (long rid in resourceIds)
                            {
                                bool stillRefdByNode = (await m_engine.FindAsync<AB_Logic_Storage_Model>(handle, _n => _n.ResourceId_ == rid)).Any();
                                bool stillRefdBySlot = (await m_engine.FindAsync<AB_Session_Storage_Model>(handle, _s => _s.InputResourceId_ == rid)).Any();
                                if (!stillRefdByNode && !stillRefdBySlot)
                                {
                                    var res = (await m_engine.FindAsync<AB_Resource_Storage_Model>(handle, _r => _r.Id_ == rid)).FirstOrDefault();
                                    if (res != null) { m_engine.Remove(handle, res); resRemoved++; }
                                }
                            }
                            if (resRemoved > 0) await m_engine.SaveChangesAsync(handle);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Delete_Session_Storage_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Success = ok,
                            SlotsRemoved = slotsRemoved,
                            ContextsRemoved = ctxRemoved,
                            NodesRemoved = nodesRemoved,
                            ResourcesRemoved = resRemoved,
                        });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AB_Log.Error("StorageGw", $"HandleMessage 예외: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
