using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder.Sharding;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using EDPFW;
using System;
using System.Collections.Generic;
using System.IO;

using System.Linq;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// Persona DB 게이트웨이. 브로커 토픽 db.persona 를 구독하고
    /// EDP_Db_Engine 호출로 직결 변환 (Phase C: CRUD 전량 이주).
    /// </summary>
    public class AB_Persona_Db_Gateway : ArtificialBuilder_EDP.Core.AB_Component
    {
        private IAB_Message_Broker? m_broker;
        private AB_Subscription_Token? m_sub;
        private EDP_Db_Engine m_engine => AB_Board.Db;

        public override void OnAttach()
        {
            try
            {
                m_broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
                m_sub = m_broker.Subscribe(AB_Persona_Db_Topics.Persona, HandleMessage);
            }
            catch (Exception ex)
            {
                AB_Log.Error("PersonaGw", $"OnAttach 실패: {ex.Message}");
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
                int handle = persona.Handle;

                switch (_msg)
                {
                    // ============================================================
                    // Settings / Lifecycle (Phase B)
                    // ============================================================
                    case AB_Get_Persona_Settings_Request req:
                    {
                        AB_Persona_Settings_Model? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            var all = await m_engine.GetAllAsync<AB_Persona_Settings_Model>(handle);
                            data = all.FirstOrDefault();
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_Persona_Settings_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Add_Persona_Settings_Request req:
                    {
                        if (handle != 0)
                        {
                            await m_engine.AddAsync(handle, req.Settings);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Add_Persona_Settings_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }
                    case AB_Save_Persona_Settings_Request req:
                    {
                        if (handle != 0)
                        {
                            m_engine.Update(handle, req.Settings);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Save_Persona_Settings_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }
                    case AB_Get_Active_Persona_Name_Request req:
                    {
                        m_broker?.Publish(new AB_Get_Active_Persona_Name_Response
                        { CorrelationId = req.CorrelationId, Name = persona.ActiveName });
                        break;
                    }
                    case AB_Get_Persona_Names_Request req:
                    {
                        m_broker?.Publish(new AB_Get_Persona_Names_Response
                        { CorrelationId = req.CorrelationId, Names = GetPersonaNames() });
                        break;
                    }
                    case AB_Rename_Persona_Request req:
                    {
                        await persona.CloseAsync();
                        File.Move($"persona/{req.OldName}.psna", $"persona/{req.NewName}.psna", overwrite: false);
                        await persona.OpenAsync(req.NewName);
                        m_broker?.Publish(new AB_Rename_Persona_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Delete_Persona_Request req:
                    {
                        await persona.CloseAsync();
                        DeletePersonaFiles(req.PersonaName);
                        var remaining = GetPersonaNames();
                        string? nextName = null;
                        if (remaining.Count > 0)
                        {
                            nextName = remaining[0];
                            await persona.OpenAsync(nextName);
                        }
                        else
                        {
                            // 0 개로 떨어지면 활성 마커도 정리 — 다음 부팅 / UI 가 첫 입력 화면으로 복귀.
                            try { if (File.Exists(ACTIVE_FILE)) File.Delete(ACTIVE_FILE); } catch { }
                        }
                        m_broker?.Publish(new AB_Delete_Persona_Response
                        { CorrelationId = req.CorrelationId, Success = true, NextPersonaName = nextName });
                        break;
                    }

                    // ============================================================
                    // Session CRUD
                    // ============================================================
                    case AB_Create_Session_Request req:
                    {
                        AB_Chat_Session_Model? session = null;
                        if (handle != 0)
                        {
                            session = new AB_Chat_Session_Model
                            { PersonaId_ = req.PersonaName, CircuitName_ = req.CircuitName };
                            if (req.TurnShardSize > 0) session.TurnShardSize_ = req.TurnShardSize;
                            await m_engine.AddAsync(handle, session);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Create_Session_Response
                        { CorrelationId = req.CorrelationId, Data = session });
                        break;
                    }
                    case AB_Get_Session_Request req:
                    {
                        AB_Chat_Session_Model? session = null;
                        if (handle != 0)
                        {
                            long sid = req.SessionId;
                            var sessions = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.Id_ == sid);
                            session = sessions.FirstOrDefault();
                        }
                        m_broker?.Publish(new AB_Get_Session_Response
                        { CorrelationId = req.CorrelationId, Data = session });
                        break;
                    }
                    case AB_Get_All_Sessions_Request req:
                    {
                        List<AB_Chat_Session_Model> result = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.GetAllAsync<AB_Chat_Session_Model>(handle);
                            foreach (var s in all)
                            {
                                if (string.IsNullOrEmpty(req.Filter)
                                    || s.Title_.Contains(req.Filter, StringComparison.OrdinalIgnoreCase)
                                    || s.CircuitName_.Contains(req.Filter, StringComparison.OrdinalIgnoreCase))
                                {
                                    result.Add(s);
                                }
                            }
                            result.Sort(CompareSessionByUpdatedAtDesc);
                        }
                        m_broker?.Publish(new AB_Get_All_Sessions_Response
                        { CorrelationId = req.CorrelationId, Data = result });
                        break;
                    }
                    case AB_Get_Sessions_Request req:
                    {
                        List<AB_Chat_Session_Model> result = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.CircuitName_ == req.CircuitName);
                            foreach (var s in all)
                            {
                                if (s.Title_.Contains(req.Filter, StringComparison.OrdinalIgnoreCase))
                                    result.Add(s);
                            }
                            result.Sort(CompareSessionByUpdatedAtDesc);
                        }
                        m_broker?.Publish(new AB_Get_Sessions_Response
                        { CorrelationId = req.CorrelationId, Data = result });
                        break;
                    }
                    case AB_Get_Session_Count_Request req:
                    {
                        int count = 0;
                        if (handle != 0)
                        {
                            var all = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.CircuitName_ == req.CircuitName);
                            count = all.Count();
                        }
                        m_broker?.Publish(new AB_Get_Session_Count_Response
                        { CorrelationId = req.CorrelationId, Count = count });
                        break;
                    }
                    case AB_Rename_Session_Request req:
                    {
                        bool ok = false;
                        if (handle != 0)
                        {
                            long sid = req.SessionId;
                            var sessions = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.Id_ == sid);
                            var session = sessions.FirstOrDefault();
                            if (session != null)
                            {
                                session.Title_ = req.NewTitle;
                                session.UpdatedAt_ = DateTime.UtcNow;
                                m_engine.Update(handle, session);
                                await m_engine.SaveChangesAsync(handle);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Rename_Session_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Copy_Session_Request req:
                    {
                        AB_Chat_Session_Model? newSession = null;
                        if (handle != 0)
                        {
                            long sid = req.SessionId;
                            var sessions = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.Id_ == sid);
                            var session = sessions.FirstOrDefault();
                            if (session != null)
                            {
                                newSession = new AB_Chat_Session_Model
                                {
                                    PersonaId_ = session.PersonaId_,
                                    CircuitName_ = session.CircuitName_,
                                    Title_ = session.Title_ + AB_Localization.I.Get("chat.copy_suffix")
                                };
                                await m_engine.AddAsync(handle, newSession);
                                await m_engine.SaveChangesAsync(handle);
                                // 메시지 복제는 context_records 기반 재설계 대기.
                            }
                        }
                        m_broker?.Publish(new AB_Copy_Session_Response
                        { CorrelationId = req.CorrelationId, Data = newSession });
                        break;
                    }
                    case AB_Move_Session_Request req:
                    {
                        bool ok = false;
                        if (handle != 0)
                        {
                            long sid = req.SessionId;
                            var sessions = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.Id_ == sid);
                            var session = sessions.FirstOrDefault();
                            if (session != null)
                            {
                                session.PersonaId_ = req.TargetPersonaName;
                                session.UpdatedAt_ = DateTime.UtcNow;
                                m_engine.Update(handle, session);
                                await m_engine.SaveChangesAsync(handle);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Move_Session_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Delete_Session_Request req:
                    {
                        bool ok = false;
                        if (handle != 0)
                        {
                            long sid = req.SessionId;
                            var sessions = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.Id_ == sid);
                            var session = sessions.FirstOrDefault();
                            if (session != null)
                            {
                                // Phase 4.4.d — session_data_pool cascade 폐기. 4 계층 storage 가 정본.
                                m_engine.Remove(handle, session);
                                await m_engine.SaveChangesAsync(handle);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Delete_Session_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Touch_Session_Request req:
                    {
                        bool ok = false;
                        if (handle != 0)
                        {
                            long sid = req.SessionId;
                            var sessions = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.Id_ == sid);
                            var session = sessions.FirstOrDefault();
                            if (session != null)
                            {
                                session.UpdatedAt_ = DateTime.UtcNow;
                                m_engine.Update(handle, session);
                                await m_engine.SaveChangesAsync(handle);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Touch_Session_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Update_Session_Title_From_First_Message_Request req:
                    {
                        string? title = null;
                        if (handle != 0)
                        {
                            long sid = req.SessionId;
                            var sessions = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.Id_ == sid);
                            var session = sessions.FirstOrDefault();
                            if (session != null)
                            {
                                // 첫 user 입력이 들어올 때 한 번 세팅.
                                // chat_messages 카운트 체크 대신 "기본 제목일 때만 갱신" 로 전환.
                                string defaultTitle = AB_Localization.I.Get("chat.new_session_title");
                                if (string.IsNullOrEmpty(session.Title_) || session.Title_ == defaultTitle)
                                {
                                    session.Title_ = req.Text.Length > 28 ? req.Text[..28] + "..." : req.Text;
                                    session.UpdatedAt_ = DateTime.UtcNow;
                                    m_engine.Update(handle, session);
                                    await m_engine.SaveChangesAsync(handle);
                                    title = session.Title_;
                                }
                            }
                        }
                        m_broker?.Publish(new AB_Update_Session_Title_From_First_Message_Response
                        { CorrelationId = req.CorrelationId, Title = title });
                        break;
                    }
                    case AB_Update_Session_Cost_Request req:
                    {
                        // 현재 스텁
                        m_broker?.Publish(new AB_Update_Session_Cost_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Get_Session_Cost_Request req:
                    {
                        // 비용 집계는 chat_messages 기반이었음 — context_records.meta_json 합산 재설계 대기.
                        m_broker?.Publish(new AB_Get_Session_Cost_Response
                        { CorrelationId = req.CorrelationId, InputTokens = 0, OutputTokens = 0, Cost = 0 });
                        break;
                    }

                    // Phase 4.4.d — session_data_pool 관련 핸들러 (Get/Put/GetEntry/Delete/CleanupOrphan) 폐기.
                    // 4 계층 storage ([[storage-layers]]) 가 영속 정본.
                    case AB_Update_Session_Turn_Shard_Size_Request req:
                    {
                        bool ok = false;
                        if (handle != 0)
                        {
                            long sid = req.SessionId;
                            var sessions = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.Id_ == sid);
                            var session = sessions.FirstOrDefault();
                            if (session != null)
                            {
                                session.TurnShardSize_ = req.NewTurnShardSize;
                                session.UpdatedAt_ = DateTime.UtcNow;
                                m_engine.Update(handle, session);
                                await m_engine.SaveChangesAsync(handle);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Update_Session_Turn_Shard_Size_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Update_Session_Circuit_Request req:
                    {
                        bool ok = false;
                        if (handle != 0)
                        {
                            long sid = req.SessionId;
                            var sessions = await m_engine.FindAsync<AB_Chat_Session_Model>(handle, _s => _s.Id_ == sid);
                            var session = sessions.FirstOrDefault();
                            if (session != null)
                            {
                                session.CircuitName_ = req.CircuitName;
                                session.UpdatedAt_ = DateTime.UtcNow;
                                m_engine.Update(handle, session);
                                await m_engine.SaveChangesAsync(handle);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Update_Session_Circuit_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }

                    // Phase 4.6 — Context_Record / Context_History 핸들러 폐기.
                    // 4 계층 storage ([[storage-layers]]) 의 Context_Storage / Node_Storage / Session_Storage 가 정본.
                    // MessageDataRef 핸들러 3 개는 Phase C 에서 폐기됨.

                    // ============================================================
                    // SavedImage
                    // ============================================================
                    case AB_Add_Saved_Image_Request req:
                    {
                        bool ok = false;
                        if (handle != 0 && req.Image != null)
                        {
                            await m_engine.AddAsync(handle, req.Image);
                            await m_engine.SaveChangesAsync(handle);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Saved_Image_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }

                    // ============================================================
                    // Vec (Chat Embedding) — Persona-owned
                    // ============================================================
                    case AB_Persona_Search_Chat_Request req:
                    {
                        var hits = new List<AB_Vec_Chat_Hit>();
                        var vs = persona.VecStore;
                        if (vs != null)
                        {
                            var raw = vs.SearchChat(persona.VecHandle, req.Query, req.TopK, req.ExcludeSessionId);
                            foreach (var (sid, nid, ti, ri, eo, dist) in raw)
                                hits.Add(new AB_Vec_Chat_Hit
                                {
                                    SessionId = long.Parse(sid),
                                    NodeId = nid,
                                    TurnIndex = ti,
                                    RefreshIndex = ri,
                                    EmissionOrder = eo,
                                    Distance = dist
                                });
                        }
                        m_broker?.Publish(new AB_Persona_Search_Chat_Response
                        { CorrelationId = req.CorrelationId, Hits = hits });
                        break;
                    }
                    case AB_Persona_Insert_Chat_Embedding_Request req:
                    {
                        if (req.Embedding != null && req.Embedding.Length > 0 && handle != 0)
                        {
                            if (!persona.IsVecInitialized())
                                persona.InitializeVec(req.Embedding.Length);
                            persona.VecStore?.InsertChatEmbedding(persona.VecHandle,
                                req.SessionId.ToString(), req.NodeId, req.TurnIndex, req.RefreshIndex, req.EmissionOrder, req.Embedding);
                        }
                        m_broker?.Publish(new AB_Persona_Insert_Chat_Embedding_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Persona_Delete_Chat_Embeddings_By_Session_Request req:
                    {
                        persona.VecStore?.DeleteChatEmbeddingsBySession(persona.VecHandle, req.SessionId.ToString());
                        m_broker?.Publish(new AB_Persona_Delete_Chat_Embeddings_By_Session_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Persona_Delete_Chat_Embedding_By_Record_Request req:
                    {
                        persona.VecStore?.DeleteChatEmbeddingByRecord(persona.VecHandle,
                            req.SessionId.ToString(), req.NodeId, req.TurnIndex, req.RefreshIndex, req.EmissionOrder);
                        m_broker?.Publish(new AB_Persona_Delete_Chat_Embedding_By_Record_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Persona_Get_Chat_Embeddings_By_Session_Request req:
                    {
                        var data = new List<AB_Chat_Embedding_Info>();
                        var vs = persona.VecStore;
                        if (vs != null)
                        {
                            var raw = vs.GetChatEmbeddingsBySession(persona.VecHandle, req.SessionId.ToString());
                            foreach (var (nid, ti, ri, eo, dim) in raw)
                                data.Add(new AB_Chat_Embedding_Info
                                {
                                    NodeId = nid,
                                    TurnIndex = ti,
                                    RefreshIndex = ri,
                                    EmissionOrder = eo,
                                    Dimensions = dim
                                });
                        }
                        m_broker?.Publish(new AB_Persona_Get_Chat_Embeddings_By_Session_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Text.StringBuilder chain = new();
                Exception? cur = ex;
                int depth = 0;
                while (cur != null && depth < 8)
                {
                    chain.Append(depth == 0 ? "" : "\n  → caused by: ");
                    chain.Append($"[{cur.GetType().Name}] {cur.Message}");
                    cur = cur.InnerException;
                    depth++;
                }
                AB_Log.Error("PersonaGw", $"{_msg.GetType().Name} 처리 실패: {chain}\n{ex.StackTrace}");
                switch (_msg)
                {
                    case AB_Add_Persona_Settings_Request req:
                        m_broker?.Publish(new AB_Add_Persona_Settings_Response
                        { CorrelationId = req.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Save_Persona_Settings_Request req:
                        m_broker?.Publish(new AB_Save_Persona_Settings_Response
                        { CorrelationId = req.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Rename_Persona_Request req:
                        m_broker?.Publish(new AB_Rename_Persona_Response
                        { CorrelationId = req.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Persona_Request req:
                        m_broker?.Publish(new AB_Delete_Persona_Response
                        { CorrelationId = req.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Create_Session_Request req:
                        m_broker?.Publish(new AB_Create_Session_Response
                        { CorrelationId = req.CorrelationId, Data = null });
                        break;
                    case AB_Get_Session_Request req:
                        m_broker?.Publish(new AB_Get_Session_Response
                        { CorrelationId = req.CorrelationId, Data = null });
                        break;
                    case AB_Get_All_Sessions_Request req:
                        m_broker?.Publish(new AB_Get_All_Sessions_Response
                        { CorrelationId = req.CorrelationId, Data = new List<AB_Chat_Session_Model>() });
                        break;
                    case AB_Get_Sessions_Request req:
                        m_broker?.Publish(new AB_Get_Sessions_Response
                        { CorrelationId = req.CorrelationId, Data = new List<AB_Chat_Session_Model>() });
                        break;
                    case AB_Get_Session_Count_Request req:
                        m_broker?.Publish(new AB_Get_Session_Count_Response
                        { CorrelationId = req.CorrelationId, Count = 0 });
                        break;
                    case AB_Rename_Session_Request req:
                        m_broker?.Publish(new AB_Rename_Session_Response
                        { CorrelationId = req.CorrelationId, Success = false });
                        break;
                    case AB_Copy_Session_Request req:
                        m_broker?.Publish(new AB_Copy_Session_Response
                        { CorrelationId = req.CorrelationId, Data = null });
                        break;
                    case AB_Move_Session_Request req:
                        m_broker?.Publish(new AB_Move_Session_Response
                        { CorrelationId = req.CorrelationId, Success = false });
                        break;
                    case AB_Delete_Session_Request req:
                        m_broker?.Publish(new AB_Delete_Session_Response
                        { CorrelationId = req.CorrelationId, Success = false });
                        break;
                    case AB_Touch_Session_Request req:
                        m_broker?.Publish(new AB_Touch_Session_Response
                        { CorrelationId = req.CorrelationId, Success = false });
                        break;
                    // Phase 4.6 — Context_Record offline path 폐기.
                }
            }
        }

        // --- 파일시스템 헬퍼 ---

        private const string PERSONA_DIR = "persona";
        private const string ACTIVE_FILE = "persona/.active";

        /// <summary>플랫 .psna + 디렉터리/core.psna 양쪽 형식 페르소나 이름 목록.</summary>
        private static List<string> GetPersonaNames()
        {
            Directory.CreateDirectory(PERSONA_DIR);
            HashSet<string> nameSet = new();
            foreach (string f in Directory.GetFiles(PERSONA_DIR, "*.psna"))
                nameSet.Add(Path.GetFileNameWithoutExtension(f));
            foreach (string d in Directory.GetDirectories(PERSONA_DIR))
            {
                if (File.Exists(Path.Combine(d, "core.psna")))
                    nameSet.Add(Path.GetFileName(d));
            }
            List<string> names = new(nameSet);
            names.Sort(StringComparer.Ordinal);
            return names;
        }

        /// <summary>플랫 / 디렉터리 양쪽 형식 페르소나 삭제. 활성 마커도 정리.</summary>
        private static void DeletePersonaFiles(string _name)
        {
            string flatPath = Path.Combine(PERSONA_DIR, $"{_name}.psna");
            if (File.Exists(flatPath)) File.Delete(flatPath);

            string dirPath = Path.Combine(PERSONA_DIR, _name);
            if (Directory.Exists(dirPath)) Directory.Delete(dirPath, recursive: true);
        }

        // --- 비교 헬퍼 ---

        private static int CompareSessionByUpdatedAtDesc(AB_Chat_Session_Model _a, AB_Chat_Session_Model _b)
            => _b.UpdatedAt_.CompareTo(_a.UpdatedAt_);

        // Phase 4.4.d — 세션풀 샤드 라우팅 헬퍼 폐기. 4 계층 storage ([[storage-layers]]) 가 정본.
        // Phase 4.6 — Context_Record / Context_History 샤드 라우팅 헬퍼 통째 폐기.

        // ShardGetRefsAsync 는 Phase C 에서 폐기됨 — AB_MessageDataRefModel 모델 자체 소멸.
    }
}
