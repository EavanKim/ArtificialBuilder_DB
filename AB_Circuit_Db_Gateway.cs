using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core;
using ArtificialBuilder_EDP.Core.Messaging;
using EDPFW;
using System;
using System.Collections.Generic;

namespace ArtificialBuilder
{
    /// <summary>
    /// 활성 circuit DB에 대한 요청을 메시지 브로커 토픽으로 받아 백엔드 호출로 변환하는 게이트웨이.
    /// 토픽: AB_Circuit_Db_Topics.ActiveCircuit.
    /// EDP 컴포넌트로 등록되며 OnAttach 시점에 브로커 구독.
    /// 옛 AB_Circuit_Db 인스턴스(AB_Db_Manager.Instance.Circuit)를 그대로 사용 — 점진 마이그레이션 단계.
    /// </summary>
    public class AB_Circuit_Db_Gateway : ArtificialBuilder_EDP.Core.AB_Component
    {
        private IAB_Message_Broker? m_broker;
        private AB_Subscription_Token? m_sub;

        /// <inheritdoc/>
        public override void OnAttach()
        {
            try
            {
                m_broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
                m_sub = m_broker.Subscribe(AB_Circuit_Db_Topics.ActiveCircuit, HandleMessage);
            }
            catch (Exception ex)
            {
                AB_Log.Error("CircuitGw", $"OnAttach 실패: {ex.Message}");
            }
        }

        /// <inheritdoc/>
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
            // 응답 메시지(자기 자신이 publish한)는 무시
            if (_msg.IsResponse) return;

            try
            {
                var circuit = AB_Board.Circuit;

                switch (_msg)
                {
                    case AB_Get_All_Characters_Request req:
                    {
                        var data = await CharactersGetAllAsync();
                        m_broker?.Publish(new AB_Get_All_Characters_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Data = data
                        });
                        break;
                    }
                    case AB_Get_Character_Request req:
                    {
                        int dbId = ActiveDbId;
                        AB_Character_Model? data = dbId == 0 ? null
                            : await AB_Board.Db.GetByIdAsync<AB_Character_Model>(dbId, req.Id);
                        m_broker?.Publish(new AB_Get_Character_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Character_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Character);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Add_Character_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Save_Character_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Update(dbId, req.Character);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Save_Character_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Delete_Character_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Remove(dbId, req.Character);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Delete_Character_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Get_All_Relationships_Request req:
                    {
                        int dbId = ActiveDbId;
                        List<AB_Character_Relationship_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Character_Relationship_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Relationships_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_All_Locations_Request req:
                    {
                        var data = await LocationsGetAllAsync();
                        m_broker?.Publish(new AB_Get_All_Locations_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_All_Location_Connections_Request req:
                    {
                        int dbId = ActiveDbId;
                        List<AB_Location_Connection_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Location_Connection_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Location_Connections_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_All_Relation_Colors_Request req:
                    {
                        int dbId = ActiveDbId;
                        List<AB_Relation_Color_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Relation_Color_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Relation_Colors_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Relation_Color_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Color);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Add_Relation_Color_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Save_Relation_Color_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Update(dbId, req.Color);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Save_Relation_Color_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Delete_Relation_Color_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Remove(dbId, req.Color);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Delete_Relation_Color_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    // ==================== H. Vec ====================
                    case AB_Is_Vec_Initialized_Request req:
                    {
                        var vs = circuit.VecStore;
                        bool init = vs != null && circuit.VecHandle != 0 && vs.IsInitialized(circuit.VecHandle);
                        m_broker?.Publish(new AB_Is_Vec_Initialized_Response
                        { CorrelationId = req.CorrelationId, Initialized = init });
                        break;
                    }
                    case AB_Get_Vec_Total_Row_Count_Request req:
                    {
                        var vs = circuit.VecStore;
                        int cnt = 0;
                        if (vs != null && circuit.VecHandle != 0)
                        {
                            try { cnt = vs.GetTotalRowCount(circuit.VecHandle); } catch { }
                        }
                        m_broker?.Publish(new AB_Get_Vec_Total_Row_Count_Response
                        { CorrelationId = req.CorrelationId, Count = cnt });
                        break;
                    }
                    case AB_Clear_All_Vec_Request req:
                    {
                        var vs = circuit.VecStore;
                        if (vs != null && circuit.VecHandle != 0)
                        {
                            try { vs.ClearAll(circuit.VecHandle); } catch { }
                        }
                        m_broker?.Publish(new AB_Clear_All_Vec_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Search_Lore_Vec_Request req:
                    {
                        var vs = circuit.VecStore;
                        var raw = vs == null ? new() : vs.SearchLore(circuit.VecHandle, req.Query, req.TopK);
                        var hits = new List<AB_Vec_Hit>(raw.Count);
                        foreach (var (id, dist) in raw) hits.Add(new AB_Vec_Hit { Id = id, Distance = dist });
                        m_broker?.Publish(new AB_Search_Lore_Vec_Response
                        { CorrelationId = req.CorrelationId, Hits = hits });
                        break;
                    }
                    case AB_Search_Chat_Vec_Request req:
                    {
                        // 채팅 임베딩은 페르소나 DB 소관 — PersonaDbProxy 경유
                        var hits = await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Persona_Db_Proxy>().SearchChatAsync(req.Query, req.TopK, req.ExcludeSessionId);
                        m_broker?.Publish(new AB_Search_Chat_Vec_Response
                        { CorrelationId = req.CorrelationId, Hits = hits });
                        break;
                    }
                    case AB_Search_CData_Vec_Request req:
                    {
                        var vs = circuit.VecStore;
                        var raw = vs == null ? new() : vs.SearchCData(circuit.VecHandle, req.Query, req.TopK);
                        var hits = new List<AB_Vec_Hit>(raw.Count);
                        foreach (var (id, dist) in raw) hits.Add(new AB_Vec_Hit { Id = id, Distance = dist });
                        m_broker?.Publish(new AB_Search_CData_Vec_Response
                        { CorrelationId = req.CorrelationId, Hits = hits });
                        break;
                    }
                    case AB_Insert_Lore_Embedding_Request req:
                    {
                        circuit.VecStore?.InsertLoreEmbedding(circuit.VecHandle, req.LoreId, req.Embedding);
                        m_broker?.Publish(new AB_Insert_Lore_Embedding_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Delete_Lore_Embedding_Request req:
                    {
                        circuit.VecStore?.DeleteLoreEmbedding(circuit.VecHandle, req.LoreId);
                        m_broker?.Publish(new AB_Delete_Lore_Embedding_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Insert_Chat_Embedding_Request req:
                    {
                        await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Persona_Db_Proxy>().InsertChatEmbeddingAsync(req.SessionId, req.NodeId,
                            req.TurnIndex, req.RefreshIndex, req.EmissionOrder, req.Embedding);
                        m_broker?.Publish(new AB_Insert_Chat_Embedding_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Delete_Chat_Embeddings_By_Session_Request req:
                    {
                        await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Persona_Db_Proxy>().DeleteChatEmbeddingsBySessionAsync(req.SessionId);
                        m_broker?.Publish(new AB_Delete_Chat_Embeddings_By_Session_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Delete_Chat_Embedding_By_Record_Request req:
                    {
                        await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Persona_Db_Proxy>().DeleteChatEmbeddingByRecordAsync(req.SessionId, req.NodeId,
                            req.TurnIndex, req.RefreshIndex, req.EmissionOrder);
                        m_broker?.Publish(new AB_Delete_Chat_Embedding_By_Record_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Get_Chat_Embeddings_By_Session_Request req:
                    {
                        var data = await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Persona_Db_Proxy>().GetChatEmbeddingsBySessionAsync(req.SessionId);
                        m_broker?.Publish(new AB_Get_Chat_Embeddings_By_Session_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Insert_CData_Embedding_Request req:
                    {
                        circuit.VecStore?.InsertCDataEmbedding(circuit.VecHandle, req.CDataId, req.Embedding);
                        m_broker?.Publish(new AB_Insert_CData_Embedding_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Open_Vec_File_Request req:
                    {
                        // .vec 파일은 InitializeVec 시점에 생성. Open 은 로그만 — Lifecycle 에 가까움.
                        if (circuit.Handle != 0 && !string.IsNullOrEmpty(circuit.ActiveName))
                            AB_Log.Debug("Vec", $"Vec file: circuit/{circuit.ActiveName}.vec");
                        m_broker?.Publish(new AB_Open_Vec_File_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                    case AB_Rename_Vec_File_Request req:
                    {
                        bool ok = RenameVecFile(req.OldName, req.NewName);
                        m_broker?.Publish(new AB_Rename_Vec_File_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Delete_Circuit_File_Request req:
                    {
                        // [[db-access]] — DB 파일 IO 는 Gateway 에서. 활성 핸들이면 close 먼저.
                        bool ok = false;
                        string? err = null;
                        try
                        {
                            var activeCircuit = AB_Board.Circuit;
                            if (activeCircuit.Handle != 0 && activeCircuit.ActiveName == req.Name)
                            {
                                await activeCircuit.CloseAsync();
                            }
                            string? circuitPath = AB_Circuit_Db.FindCircuitFile(req.Name);
                            if (circuitPath != null && File.Exists(circuitPath))
                            {
                                File.Delete(circuitPath);
                            }
                            string vecPath = Path.Combine("circuit", $"{req.Name}.vec");
                            if (File.Exists(vecPath)) File.Delete(vecPath);
                            ok = true;
                        }
                        catch (Exception ex)
                        {
                            err = ex.Message;
                            AB_Log.Warn("CircuitGw", $"DeleteCircuitFile 실패 name={req.Name}: {ex.Message}");
                        }
                        m_broker?.Publish(new AB_Delete_Circuit_File_Response
                        { CorrelationId = req.CorrelationId, Success = ok, Error = err });
                        break;
                    }
                    // =================================================
                    case AB_Get_All_Session_Data_Request req:
                    {
                        int dbId = ActiveDbId;
                        List<AB_Character_Data_Model> data = new();
                        if (dbId != 0)
                        {
                            long sid = req.SessionId;
                            var all = await AB_Board.Db.FindAsync<AB_Character_Data_Model>(dbId, _d => _d.SessionId_ == sid);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Session_Data_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_All_Data_Categories_Request req:
                    {
                        int dbId = ActiveDbId;
                        List<AB_Data_Category_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Data_Category_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Data_Categories_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Delete_Session_Data_Request req:
                    {
                        bool ok = await CharacterDataDeleteBySessionAsync(req.SessionId);
                        m_broker?.Publish(new AB_Delete_Session_Data_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Delete_Character_Data_By_Message_Request req:
                    {
                        bool ok = await CharacterDataDeleteByMessageAsync(req.MessageId);
                        m_broker?.Publish(new AB_Delete_Character_Data_By_Message_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Delete_Character_Data_From_Message_Request req:
                    {
                        bool ok = await CharacterDataDeleteFromMessageAsync(req.FromMessageId);
                        m_broker?.Publish(new AB_Delete_Character_Data_From_Message_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Copy_Circuit_Data_To_Session_Request req:
                    {
                        bool ok = await CharacterDataCopyCircuitToSessionAsync(req.SessionId);
                        m_broker?.Publish(new AB_Copy_Circuit_Data_To_Session_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_Character_Data_By_Category_Request req:
                    {
                        int dbId = ActiveDbId;
                        List<AB_Character_Data_Model> data = new();
                        if (dbId != 0)
                        {
                            string cid = req.CharacterId;
                            long? sid = req.SessionId == 0L ? (long?)null : req.SessionId;
                            string catId = req.CategoryId;
                            var all = await AB_Board.Db.FindAsync<AB_Character_Data_Model>(dbId,
                                _d => _d.CharacterId_ == cid && _d.SessionId_ == sid && _d.CategoryId_ == catId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_Character_Data_By_Category_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Upsert_Character_Data_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            var item = new AB_Character_Data_Model
                            {
                                CharacterId_ = req.CharacterId,
                                SessionId_ = req.SessionId == 0L ? (long?)null : req.SessionId,
                                CategoryId_ = req.CategoryId,
                                FieldName_ = req.FieldName,
                                FieldValue_ = req.FieldValue,
                                Narrative_ = req.Narrative,
                                Source_ = req.Source,
                                MessageId_ = req.MessageId
                            };
                            await AB_Board.Db.AddAsync(dbId, item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Upsert_Character_Data_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Delete_Character_Data_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            var item = await AB_Board.Db.GetByIdAsync<AB_Character_Data_Model>(dbId, req.Id);
                            if (item != null)
                            {
                                AB_Board.Db.Remove(dbId, item);
                                await AB_Board.Db.SaveChangesAsync(dbId);
                            }
                        }
                        m_broker?.Publish(new AB_Delete_Character_Data_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Add_Data_Category_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Category);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Add_Data_Category_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Get_All_Patterns_Request req:
                    {
                        var data = await PatternsGetAllAsync();
                        m_broker?.Publish(new AB_Get_All_Patterns_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_All_Lore_Entries_Request req:
                    {
                        var data = await LoreGetAllAsync();
                        m_broker?.Publish(new AB_Get_All_Lore_Entries_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Data = data
                        });
                        break;
                    }
                    case AB_Get_Lore_Entry_Request req:
                    {
                        int dbId = ActiveDbId;
                        AB_Lore_Entry_Model? data = dbId == 0 ? null
                            : await AB_Board.Db.GetByIdAsync<AB_Lore_Entry_Model>(dbId, req.Id);
                        m_broker?.Publish(new AB_Get_Lore_Entry_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Data = data
                        });
                        break;
                    }
                    case AB_Add_Lore_Entry_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Entry);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Add_Lore_Entry_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Success = dbId != 0
                        });
                        break;
                    }
                    case AB_Save_Lore_Entry_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            req.Entry.UpdatedAt_ = DateTime.UtcNow;
                            AB_Board.Db.Update(dbId, req.Entry);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Save_Lore_Entry_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Success = dbId != 0
                        });
                        break;
                    }
                    case AB_Delete_Lore_Entry_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Remove(dbId, req.Entry);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Delete_Lore_Entry_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Success = dbId != 0
                        });
                        break;
                    }
                    case AB_Find_Matching_Lore_Request req:
                    {
                        var data = await LoreFindMatchingAsync(req.Text);
                        m_broker?.Publish(new AB_Find_Matching_Lore_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Data = data
                        });
                        break;
                    }
                    case AB_Get_Circuit_Settings_Request req:
                    {
                        int dbId = ActiveDbId;
                        AB_Circuit_Settings_Model? data = dbId == 0 ? null
                            : await AB_Board.Db.GetByIdAsync<AB_Circuit_Settings_Model>(dbId, 1L);
                        m_broker?.Publish(new AB_Get_Circuit_Settings_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Data = data,
                            IsOk = data != null,
                            Error = data != null ? null : (dbId == 0 ? "no active circuit" : "settings load failed")
                        });
                        break;
                    }
                    case AB_Save_Settings_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Update(dbId, req.Settings);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Save_Settings_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Success = dbId != 0
                        });
                        break;
                    }
                    case AB_Add_Settings_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Settings);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Add_Settings_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Success = dbId != 0
                        });
                        break;
                    }
                    case AB_Get_All_Asset_Metadata_Request req:
                    {
                        var data = await AssetsGetAllMetadataAsync();
                        m_broker?.Publish(new AB_Get_All_Asset_Metadata_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Data = data
                        });
                        break;
                    }
                    case AB_Get_All_Assets_Request req:
                    {
                        var data = await AssetsGetAllAsync();
                        m_broker?.Publish(new AB_Get_All_Assets_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_Asset_Data_Request req:
                    {
                        int dbId = ActiveDbId;
                        byte[]? data = null;
                        if (dbId != 0)
                        {
                            var found = await AB_Board.Db.FindAsync<AB_Circuit_Asset_Model>(dbId, _a => _a.Id_.ToString() == req.AssetId);
                            data = found.FirstOrDefault()?.Data_;
                        }
                        m_broker?.Publish(new AB_Get_Asset_Data_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_Asset_Request req:
                    {
                        int dbId = ActiveDbId;
                        AB_Circuit_Asset_Model? data = dbId == 0 ? null
                            : await AB_Board.Db.GetByIdAsync<AB_Circuit_Asset_Model>(dbId, req.Id);
                        m_broker?.Publish(new AB_Get_Asset_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_Asset_By_Name_Request req:
                    {
                        int dbId = ActiveDbId;
                        AB_Circuit_Asset_Model? data = null;
                        if (dbId != 0)
                        {
                            var found = await AB_Board.Db.FindAsync<AB_Circuit_Asset_Model>(dbId, _a => _a.Name_ == req.Name);
                            data = found.FirstOrDefault();
                        }
                        m_broker?.Publish(new AB_Get_Asset_By_Name_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Asset_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Asset);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Add_Asset_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Add_Assets_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddRangeAsync(dbId, req.Assets);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Add_Assets_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0, AddedCount = req.Assets.Count });
                        break;
                    }
                    case AB_Delete_Asset_Request req:
                    {
                        int dbId = ActiveDbId;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Remove(dbId, req.Asset);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Delete_Asset_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Find_Assets_By_Path_Prefix_Request req:
                    {
                        int dbId = ActiveDbId;
                        List<AB_Circuit_Asset_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Circuit_Asset_Model>(dbId);
                            foreach (var a in all)
                            {
                                if (a.Name_.StartsWith(req.Prefix, StringComparison.OrdinalIgnoreCase))
                                    data.Add(a);
                            }
                        }
                        m_broker?.Publish(new AB_Find_Assets_By_Path_Prefix_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                string detail = ex.Message;
                Exception? inner = ex.InnerException;
                while (inner != null)
                {
                    detail += $" <- {inner.GetType().Name}: {inner.Message}";
                    inner = inner.InnerException;
                }
                AB_Log.Error("CircuitGw", $"{_msg.GetType().Name} 처리 실패: {detail}");
                // 모든 미처리 요청에 대해 범용 에러 응답 발행 (timeout 방지)
                PublishFallbackError(_msg, detail);
                // write 계열은 추가 구체 응답 발행
                switch (_msg)
                {
                    case AB_Save_Settings_Request sreq:
                        m_broker?.Publish(new AB_Save_Settings_Response
                        {
                            CorrelationId = sreq.CorrelationId,
                            Success = false,
                            Error = ex.Message
                        });
                        break;
                    case AB_Add_Settings_Request areq:
                        m_broker?.Publish(new AB_Add_Settings_Response
                        {
                            CorrelationId = areq.CorrelationId,
                            Success = false,
                            Error = ex.Message
                        });
                        break;
                    case AB_Add_Lore_Entry_Request aLore:
                        m_broker?.Publish(new AB_Add_Lore_Entry_Response
                        { CorrelationId = aLore.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Save_Lore_Entry_Request sLore:
                        m_broker?.Publish(new AB_Save_Lore_Entry_Response
                        { CorrelationId = sLore.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Lore_Entry_Request dLore:
                        m_broker?.Publish(new AB_Delete_Lore_Entry_Response
                        { CorrelationId = dLore.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Add_Asset_Request aA:
                        m_broker?.Publish(new AB_Add_Asset_Response
                        { CorrelationId = aA.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Add_Assets_Request aAs:
                        m_broker?.Publish(new AB_Add_Assets_Response
                        { CorrelationId = aAs.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Asset_Request dA:
                        m_broker?.Publish(new AB_Delete_Asset_Response
                        { CorrelationId = dA.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Add_Character_Request aChar:
                        m_broker?.Publish(new AB_Add_Character_Response
                        { CorrelationId = aChar.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Save_Character_Request sChar:
                        m_broker?.Publish(new AB_Save_Character_Response
                        { CorrelationId = sChar.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Character_Request dChar:
                        m_broker?.Publish(new AB_Delete_Character_Response
                        { CorrelationId = dChar.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Add_Relation_Color_Request aRC:
                        m_broker?.Publish(new AB_Add_Relation_Color_Response
                        { CorrelationId = aRC.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Save_Relation_Color_Request sRC:
                        m_broker?.Publish(new AB_Save_Relation_Color_Response
                        { CorrelationId = sRC.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Relation_Color_Request dRC:
                        m_broker?.Publish(new AB_Delete_Relation_Color_Response
                        { CorrelationId = dRC.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Session_Data_Request dSd:
                        m_broker?.Publish(new AB_Delete_Session_Data_Response
                        { CorrelationId = dSd.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Character_Data_By_Message_Request dCdbm:
                        m_broker?.Publish(new AB_Delete_Character_Data_By_Message_Response
                        { CorrelationId = dCdbm.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Character_Data_From_Message_Request dCdfm:
                        m_broker?.Publish(new AB_Delete_Character_Data_From_Message_Response
                        { CorrelationId = dCdfm.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Copy_Circuit_Data_To_Session_Request cpd:
                        m_broker?.Publish(new AB_Copy_Circuit_Data_To_Session_Response
                        { CorrelationId = cpd.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Upsert_Character_Data_Request uCd:
                        m_broker?.Publish(new AB_Upsert_Character_Data_Response
                        { CorrelationId = uCd.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Character_Data_Request dCd:
                        m_broker?.Publish(new AB_Delete_Character_Data_Response
                        { CorrelationId = dCd.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Add_Data_Category_Request aDc:
                        m_broker?.Publish(new AB_Add_Data_Category_Response
                        { CorrelationId = aDc.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    // ---- Vec write 계열 실패 응답 ----
                    case AB_Clear_All_Vec_Request cav:
                        m_broker?.Publish(new AB_Clear_All_Vec_Response
                        { CorrelationId = cav.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Insert_Lore_Embedding_Request ile:
                        m_broker?.Publish(new AB_Insert_Lore_Embedding_Response
                        { CorrelationId = ile.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Lore_Embedding_Request dle:
                        m_broker?.Publish(new AB_Delete_Lore_Embedding_Response
                        { CorrelationId = dle.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Insert_Chat_Embedding_Request ice:
                        m_broker?.Publish(new AB_Insert_Chat_Embedding_Response
                        { CorrelationId = ice.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Chat_Embeddings_By_Session_Request dcebs:
                        m_broker?.Publish(new AB_Delete_Chat_Embeddings_By_Session_Response
                        { CorrelationId = dcebs.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Chat_Embedding_By_Record_Request dcebr:
                        m_broker?.Publish(new AB_Delete_Chat_Embedding_By_Record_Response
                        { CorrelationId = dcebr.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Insert_CData_Embedding_Request icde:
                        m_broker?.Publish(new AB_Insert_CData_Embedding_Response
                        { CorrelationId = icde.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Open_Vec_File_Request ovf:
                        m_broker?.Publish(new AB_Open_Vec_File_Response
                        { CorrelationId = ovf.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Rename_Vec_File_Request rvf:
                        m_broker?.Publish(new AB_Rename_Vec_File_Response
                        { CorrelationId = rvf.CorrelationId, Success = false, Error = ex.Message });
                        break;
                    case AB_Delete_Circuit_File_Request dcf:
                        m_broker?.Publish(new AB_Delete_Circuit_File_Response
                        { CorrelationId = dcf.CorrelationId, Success = false, Error = ex.Message });
                        break;
                }
            }
        }

        /// <summary>
        /// catch 블록에서 호출 — 매칭되는 Response 타입으로 에러 응답 발행 (timeout 방지).
        /// </summary>
        private void PublishFallbackError(AB_Message _req, string _error)
        {
            try
            {
                AB_Message? resp = _req switch
                {
                    AB_Get_Circuit_Settings_Request r => new AB_Get_Circuit_Settings_Response
                    { CorrelationId = r.CorrelationId, IsOk = false, Error = _error },
                    AB_Get_All_Characters_Request r => new AB_Get_All_Characters_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_Character_Request r => new AB_Get_Character_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_All_Lore_Entries_Request r => new AB_Get_All_Lore_Entries_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_Lore_Entry_Request r => new AB_Get_Lore_Entry_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_All_Assets_Request r => new AB_Get_All_Assets_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_Asset_Request r => new AB_Get_Asset_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_Asset_By_Name_Request r => new AB_Get_Asset_By_Name_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_Asset_Data_Request r => new AB_Get_Asset_Data_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_All_Asset_Metadata_Request r => new AB_Get_All_Asset_Metadata_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_All_Relationships_Request r => new AB_Get_All_Relationships_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_All_Locations_Request r => new AB_Get_All_Locations_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_All_Location_Connections_Request r => new AB_Get_All_Location_Connections_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_All_Relation_Colors_Request r => new AB_Get_All_Relation_Colors_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_All_Patterns_Request r => new AB_Get_All_Patterns_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_All_Data_Categories_Request r => new AB_Get_All_Data_Categories_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_All_Session_Data_Request r => new AB_Get_All_Session_Data_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_Character_Data_By_Category_Request r => new AB_Get_Character_Data_By_Category_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Find_Matching_Lore_Request r => new AB_Find_Matching_Lore_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Find_Assets_By_Path_Prefix_Request r => new AB_Find_Assets_By_Path_Prefix_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Is_Vec_Initialized_Request r => new AB_Is_Vec_Initialized_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_Vec_Total_Row_Count_Request r => new AB_Get_Vec_Total_Row_Count_Response
                    { CorrelationId = r.CorrelationId },
                    _ => null
                };
                if (resp != null)
                    m_broker?.Publish(resp);
            }
            catch { }
        }

        // --- Lore 헬퍼 (Engine 직결) ---

        private static int ActiveDbId
        {
            get { return AB_Board.Circuit.Handle; }
        }

        private static async System.Threading.Tasks.Task<List<AB_Lore_Entry_Model>> LoreGetAllAsync()
        {
            List<AB_Lore_Entry_Model> result = new();
            int dbId = ActiveDbId;
            if (dbId == 0) return result;
            var all = await AB_Board.Db.GetAllAsync<AB_Lore_Entry_Model>(dbId);
            result.AddRange(all);
            result.Sort(CompareLoreByPriorityThenName);
            return result;
        }

        private static async System.Threading.Tasks.Task<List<AB_Lore_Entry_Model>> LoreFindMatchingAsync(string _text)
        {
            List<AB_Lore_Entry_Model> result = new();
            int dbId = ActiveDbId;
            if (dbId == 0) return result;
            var all = await AB_Board.Db.GetAllAsync<AB_Lore_Entry_Model>(dbId);
            string textLower = _text.ToLowerInvariant();
            foreach (AB_Lore_Entry_Model entry in all)
            {
                if (!entry.Enabled_) continue;
                string[] keywords = entry.Keywords_.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string keyword in keywords)
                {
                    if (textLower.Contains(keyword.ToLowerInvariant()))
                    {
                        result.Add(entry);
                        break;
                    }
                }
            }
            result.Sort(CompareLoreByPriorityThenName);
            return result;
        }

        private static int CompareLoreByPriorityThenName(AB_Lore_Entry_Model _a, AB_Lore_Entry_Model _b)
        {
            int cmp = _a.Priority_.CompareTo(_b.Priority_);
            if (cmp != 0) return cmp;
            return string.Compare(_a.Name_, _b.Name_, StringComparison.Ordinal);
        }

        // --- Character/Location 헬퍼 ---

        private static async System.Threading.Tasks.Task<List<AB_Character_Model>> CharactersGetAllAsync()
        {
            List<AB_Character_Model> result = new();
            int dbId = ActiveDbId;
            if (dbId == 0) return result;
            var all = await AB_Board.Db.GetAllAsync<AB_Character_Model>(dbId);
            result.AddRange(all);
            result.Sort(CompareCharacterBySortOrderThenName);
            return result;
        }

        private static async System.Threading.Tasks.Task<List<AB_Location_Model>> LocationsGetAllAsync()
        {
            List<AB_Location_Model> result = new();
            int dbId = ActiveDbId;
            if (dbId == 0) return result;
            var all = await AB_Board.Db.GetAllAsync<AB_Location_Model>(dbId);
            result.AddRange(all);
            result.Sort(CompareLocationByName);
            return result;
        }

        private static int CompareCharacterBySortOrderThenName(AB_Character_Model _a, AB_Character_Model _b)
        {
            int cmp = _a.SortOrder_.CompareTo(_b.SortOrder_);
            if (cmp != 0) return cmp;
            return string.Compare(_a.Name_, _b.Name_, StringComparison.Ordinal);
        }

        private static int CompareLocationByName(AB_Location_Model _a, AB_Location_Model _b)
        {
            return string.Compare(_a.Name_, _b.Name_, StringComparison.Ordinal);
        }

        // --- Pattern 헬퍼 ---

        private static async System.Threading.Tasks.Task<List<AB_Pattern_Config_Model>> PatternsGetAllAsync()
        {
            List<AB_Pattern_Config_Model> result = new();
            int dbId = ActiveDbId;
            if (dbId == 0) return result;
            var all = await AB_Board.Db.GetAllAsync<AB_Pattern_Config_Model>(dbId);
            result.AddRange(all);
            result.Sort(ComparePatternByType);
            return result;
        }

        private static int ComparePatternByType(AB_Pattern_Config_Model _a, AB_Pattern_Config_Model _b)
        {
            return string.Compare(_a.PatternType_, _b.PatternType_, StringComparison.Ordinal);
        }

        // --- Vec 파일 헬퍼 ---

        private static bool RenameVecFile(string _oldName, string _newName)
        {
            if (string.IsNullOrEmpty(_oldName) || string.IsNullOrEmpty(_newName)) return false;
            if (_oldName == _newName) return true;
            string oldPath = System.IO.Path.Combine("circuit", $"{_oldName}.vec");
            string newPath = System.IO.Path.Combine("circuit", $"{_newName}.vec");
            if (!System.IO.File.Exists(oldPath)) return false;
            if (System.IO.File.Exists(newPath)) return false;
            try
            {
                System.IO.File.Move(oldPath, newPath);
                AB_Log.Debug("Vec", $"Vec file renamed: {oldPath} → {newPath}");
                return true;
            }
            catch (Exception ex)
            {
                AB_Log.Error("Vec", $"Vec file rename failed: {ex.Message}");
                return false;
            }
        }

        // --- Asset 헬퍼 ---

        private static async System.Threading.Tasks.Task<List<AB_Circuit_Asset_Model>> AssetsGetAllAsync()
        {
            List<AB_Circuit_Asset_Model> result = new();
            int dbId = ActiveDbId;
            if (dbId == 0) return result;
            var all = await AB_Board.Db.GetAllAsync<AB_Circuit_Asset_Model>(dbId);
            result.AddRange(all);
            result.Sort(CompareAssetByName);
            return result;
        }

        private static async System.Threading.Tasks.Task<List<AB_Circuit_Asset_Model>> AssetsGetAllMetadataAsync()
        {
            List<AB_Circuit_Asset_Model> result = new();
            int dbId = ActiveDbId;
            if (dbId == 0) return result;
            result = await AB_Board.Db.SelectAsync<AB_Circuit_Asset_Model, AB_Circuit_Asset_Model>(
                dbId, _a => new AB_Circuit_Asset_Model
                {
                    Id_ = _a.Id_,
                    Name_ = _a.Name_,
                    FileName_ = _a.FileName_,
                    AssetType_ = _a.AssetType_,
                    MimeType_ = _a.MimeType_,
                    FileSize_ = _a.FileSize_,
                    CreatedAt_ = _a.CreatedAt_
                });
            result.Sort(CompareAssetByName);
            return result;
        }

        private static int CompareAssetByName(AB_Circuit_Asset_Model _a, AB_Circuit_Asset_Model _b)
        {
            return string.Compare(_a.Name_, _b.Name_, StringComparison.Ordinal);
        }

        // --- CharacterData 삭제 헬퍼 ---

        private static async System.Threading.Tasks.Task<bool> CharacterDataDeleteBySessionAsync(long _sessionId)
        {
            int dbId = ActiveDbId;
            if (dbId == 0) return false;
            var data = await AB_Board.Db.FindAsync<AB_Character_Data_Model>(dbId, _d => _d.SessionId_ == _sessionId);
            foreach (var d in data) AB_Board.Db.Remove(dbId, d);
            await AB_Board.Db.SaveChangesAsync(dbId);
            return true;
        }

        private static async System.Threading.Tasks.Task<bool> CharacterDataDeleteByMessageAsync(long _messageId)
        {
            int dbId = ActiveDbId;
            if (dbId == 0) return false;
            var data = await AB_Board.Db.FindAsync<AB_Character_Data_Model>(dbId, _d => _d.MessageId_ == _messageId);
            foreach (var d in data) AB_Board.Db.Remove(dbId, d);
            await AB_Board.Db.SaveChangesAsync(dbId);
            return true;
        }

        private static async System.Threading.Tasks.Task<bool> CharacterDataDeleteFromMessageAsync(long _fromMessageId)
        {
            int dbId = ActiveDbId;
            if (dbId == 0) return false;
            var data = await AB_Board.Db.FindAsync<AB_Character_Data_Model>(dbId,
                _d => _d.MessageId_ != null && _d.MessageId_ >= _fromMessageId);
            foreach (var d in data) AB_Board.Db.Remove(dbId, d);
            await AB_Board.Db.SaveChangesAsync(dbId);
            return true;
        }

        private static async System.Threading.Tasks.Task<bool> CharacterDataCopyCircuitToSessionAsync(long _sessionId)
        {
            int dbId = ActiveDbId;
            if (dbId == 0) return false;
            var circuitData = await AB_Board.Db.FindAsync<AB_Character_Data_Model>(dbId, _d => _d.SessionId_ == null);
            foreach (var d in circuitData)
            {
                await AB_Board.Db.AddAsync(dbId, new AB_Character_Data_Model
                {
                    CharacterId_ = d.CharacterId_,
                    SessionId_ = _sessionId,
                    CategoryId_ = d.CategoryId_,
                    FieldName_ = d.FieldName_,
                    FieldValue_ = d.FieldValue_,
                    Narrative_ = d.Narrative_,
                    Source_ = d.Source_
                });
            }
            await AB_Board.Db.SaveChangesAsync(dbId);
            return true;
        }
    }
}

namespace ArtificialBuilder
{
    using ArtificialBuilder.Models;
    using ArtificialBuilder.Requests;
    using ArtificialBuilder_EDP.Components;
    using ArtificialBuilder_EDP.Core.Messaging;
    using EDPFW;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// circuit-home-logic-graph-runtime-db-proxy sub 3 — v2 슬롯 / hosted_logic_slot_value 게이트웨이.
    /// AB_Circuit_Db_Gateway 본체 와 동일 토픽 (AB_Circuit_Db_Topics.ActiveCircuit). 분리 등록.
    /// </summary>
    public class AB_Circuit_Db_V2_Slots_Gateway : ArtificialBuilder_EDP.Core.AB_Component
    {
        private IAB_Message_Broker? m_broker;
        private AB_Subscription_Token? m_sub;

        public override void OnAttach()
        {
            try
            {
                m_broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
                m_sub = m_broker.Subscribe(AB_Circuit_Db_Topics.ActiveCircuit, HandleMessage);
            }
            catch (Exception ex)
            {
                AB_Log.Error("CircuitGwV2", $"OnAttach 실패: {ex.Message}");
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
                int dbId = AB_Board.Circuit.Handle;

                switch (_msg)
                {
                    case AB_Get_All_Circuit_Input_Slots_Request req:
                    {
                        System.Collections.Generic.List<AB_Circuit_Input_Slot_Model> data = new();
                        if (dbId != 0) data.AddRange(await AB_Board.Db.GetAllAsync<AB_Circuit_Input_Slot_Model>(dbId));
                        m_broker?.Publish(new AB_Get_All_Circuit_Input_Slots_Response { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Circuit_Input_Slot_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0) { await AB_Board.Db.AddAsync(dbId, req.Item); await AB_Board.Db.SaveChangesAsync(dbId); ok = true; }
                        m_broker?.Publish(new AB_Add_Circuit_Input_Slot_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Save_Circuit_Input_Slot_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0) { AB_Board.Db.Update(dbId, req.Item); await AB_Board.Db.SaveChangesAsync(dbId); ok = true; }
                        m_broker?.Publish(new AB_Save_Circuit_Input_Slot_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Remove_Circuit_Input_Slot_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            var existing = await AB_Board.Db.GetByIdAsync<AB_Circuit_Input_Slot_Model>(dbId, req.Slot_Id);
                            if (existing != null) { AB_Board.Db.Remove(dbId, existing); await AB_Board.Db.SaveChangesAsync(dbId); ok = true; }
                        }
                        m_broker?.Publish(new AB_Remove_Circuit_Input_Slot_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_All_Circuit_Output_Slots_Request req:
                    {
                        System.Collections.Generic.List<AB_Circuit_Output_Slot_Model> data = new();
                        if (dbId != 0) data.AddRange(await AB_Board.Db.GetAllAsync<AB_Circuit_Output_Slot_Model>(dbId));
                        m_broker?.Publish(new AB_Get_All_Circuit_Output_Slots_Response { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Circuit_Output_Slot_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0) { await AB_Board.Db.AddAsync(dbId, req.Item); await AB_Board.Db.SaveChangesAsync(dbId); ok = true; }
                        m_broker?.Publish(new AB_Add_Circuit_Output_Slot_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Save_Circuit_Output_Slot_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0) { AB_Board.Db.Update(dbId, req.Item); await AB_Board.Db.SaveChangesAsync(dbId); ok = true; }
                        m_broker?.Publish(new AB_Save_Circuit_Output_Slot_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Remove_Circuit_Output_Slot_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            var existing = await AB_Board.Db.GetByIdAsync<AB_Circuit_Output_Slot_Model>(dbId, req.Slot_Id);
                            if (existing != null) { AB_Board.Db.Remove(dbId, existing); await AB_Board.Db.SaveChangesAsync(dbId); ok = true; }
                        }
                        m_broker?.Publish(new AB_Remove_Circuit_Output_Slot_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_All_Hosted_Logic_Slot_Values_Request req:
                    {
                        System.Collections.Generic.List<AB_Circuit_Hosted_Logic_Slot_Value_Model> data = new();
                        if (dbId != 0) data.AddRange(await AB_Board.Db.GetAllAsync<AB_Circuit_Hosted_Logic_Slot_Value_Model>(dbId));
                        m_broker?.Publish(new AB_Get_All_Hosted_Logic_Slot_Values_Response { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_Hosted_Logic_Slot_Value_Request req:
                    {
                        AB_Circuit_Hosted_Logic_Slot_Value_Model? data = null;
                        if (dbId != 0)
                        {
                            long logicId = req.Logic_Id;
                            long slotId = req.Slot_Id;
                            var all = await AB_Board.Db.FindAsync<AB_Circuit_Hosted_Logic_Slot_Value_Model>(dbId,
                                _v => _v.LogicId_ == logicId && _v.SlotId_ == slotId);
                            foreach (var v in all) { data = v; break; }
                        }
                        m_broker?.Publish(new AB_Get_Hosted_Logic_Slot_Value_Response { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_All_Hosted_Logics_Request req:
                    {
                        System.Collections.Generic.List<AB_Circuit_Hosted_Logic_Model> data = new();
                        if (dbId != 0) data.AddRange(await AB_Board.Db.GetAllAsync<AB_Circuit_Hosted_Logic_Model>(dbId));
                        m_broker?.Publish(new AB_Get_All_Hosted_Logics_Response { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Hosted_Logic_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0) { await AB_Board.Db.AddAsync(dbId, req.Item); await AB_Board.Db.SaveChangesAsync(dbId); ok = true; }
                        m_broker?.Publish(new AB_Add_Hosted_Logic_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Remove_Hosted_Logic_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            var existing = await AB_Board.Db.GetByIdAsync<AB_Circuit_Hosted_Logic_Model>(dbId, req.Hosted_Logic_Id);
                            if (existing != null) { AB_Board.Db.Remove(dbId, existing); await AB_Board.Db.SaveChangesAsync(dbId); ok = true; }
                        }
                        m_broker?.Publish(new AB_Remove_Hosted_Logic_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Set_Hosted_Logic_Slot_Value_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            var existing = await AB_Board.Db.FindAsync<AB_Circuit_Hosted_Logic_Slot_Value_Model>(dbId,
                                _v => _v.LogicId_ == req.Item.LogicId_ && _v.SlotId_ == req.Item.SlotId_);
                            bool found = false;
                            foreach (var v in existing)
                            {
                                v.ValueJson_ = req.Item.ValueJson_;
                                AB_Board.Db.Update(dbId, v);
                                found = true;
                                break;
                            }
                            if (!found) await AB_Board.Db.AddAsync(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Set_Hosted_Logic_Slot_Value_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }

                    // --- circuit-as-node-graph sub 2 (2026-05-07) — hosted_logic 슬롯 매핑 + UI 매핑 atomic replace ---

                    case AB_Get_Hosted_Logic_Slot_Mappings_Request req:
                    {
                        System.Collections.Generic.List<AB_Hosted_Slot_Mapping_Model> data = new();
                        if (dbId != 0)
                        {
                            long hostedLogicId = req.Hosted_Logic_Id;
                            var found = await AB_Board.Db.FindAsync<AB_Hosted_Slot_Mapping_Model>(dbId,
                                _m => _m.HostedLogicId_ == hostedLogicId);
                            data.AddRange(found);
                        }
                        m_broker?.Publish(new AB_Get_Hosted_Logic_Slot_Mappings_Response { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Replace_Hosted_Logic_Slot_Mappings_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            long hostedLogicId = req.Hosted_Logic_Id;
                            var existing = await AB_Board.Db.FindAsync<AB_Hosted_Slot_Mapping_Model>(dbId,
                                _m => _m.HostedLogicId_ == hostedLogicId);
                            foreach (var row in existing) AB_Board.Db.Remove(dbId, row);
                            foreach (var item in req.Items)
                            {
                                item.HostedLogicId_ = hostedLogicId;
                                item.UpdatedAt_ = System.DateTime.UtcNow;
                                await AB_Board.Db.AddAsync(dbId, item);
                            }
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Replace_Hosted_Logic_Slot_Mappings_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_Hosted_Logic_Ui_Mappings_Request req:
                    {
                        System.Collections.Generic.List<AB_Hosted_Ui_Mapping_Model> data = new();
                        if (dbId != 0)
                        {
                            long hostedLogicId = req.Hosted_Logic_Id;
                            var found = await AB_Board.Db.FindAsync<AB_Hosted_Ui_Mapping_Model>(dbId,
                                _m => _m.HostedLogicId_ == hostedLogicId);
                            data.AddRange(found);
                        }
                        m_broker?.Publish(new AB_Get_Hosted_Logic_Ui_Mappings_Response { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Replace_Hosted_Logic_Ui_Mappings_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            long hostedLogicId = req.Hosted_Logic_Id;
                            var existing = await AB_Board.Db.FindAsync<AB_Hosted_Ui_Mapping_Model>(dbId,
                                _m => _m.HostedLogicId_ == hostedLogicId);
                            foreach (var row in existing) AB_Board.Db.Remove(dbId, row);
                            foreach (var item in req.Items)
                            {
                                item.HostedLogicId_ = hostedLogicId;
                                item.UpdatedAt_ = System.DateTime.UtcNow;
                                await AB_Board.Db.AddAsync(dbId, item);
                            }
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Replace_Hosted_Logic_Ui_Mappings_Response { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AB_Log.Error("CircuitGwV2", $"HandleMessage 실패: {ex.Message}");
            }
        }
    }
}
