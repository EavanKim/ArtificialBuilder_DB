using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtificialBuilder_EDP.Core;

namespace ArtificialBuilder
{
    /// <summary>
    /// Persona DB 프록시. 브로커 + AB_Persona_Db_Gateway 경유 호출.
    /// UI/서비스 계층에서 직접 DB 호출 대신 이 프록시를 사용.
    /// </summary>
    public class AB_Persona_Db_Proxy : ArtificialBuilder_EDP.Core.AB_Object
    {
        private TimeSpan m_defaultTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan DefaultTimeout
        {
            get { return m_defaultTimeout; }
            set { m_defaultTimeout = value; }
        }

        private IAB_Message_Broker GetBroker()
            => ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();

        // ============================================================
        // Settings / Lifecycle meta
        // ============================================================

        /// <summary>활성 페르소나 이름.</summary>
        public async Task<string?> GetActiveNameAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Active_Persona_Name_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Active_Persona_Name_Response>(req, DefaultTimeout);
            return resp.Name;
        }

        /// <summary>페르소나 설정 조회.</summary>
        public async Task<AB_Db_Result<AB_Persona_Settings_Model>> GetSettingsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Persona_Settings_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Persona_Settings_Response>(req, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Persona_Settings_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Persona_Settings_Model>.NotFound();
        }

        /// <summary>페르소나 설정 추가.</summary>
        public async Task AddSettingsAsync(AB_Persona_Settings_Model _settings)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Persona_Settings_Request>();
            req.Settings = _settings;
            await GetBroker().PublishAndWaitAsync<AB_Add_Persona_Settings_Response>(req, DefaultTimeout);
        }

        /// <summary>페르소나 설정 저장.</summary>
        public async Task SaveSettingsAsync(AB_Persona_Settings_Model _settings)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Persona_Settings_Request>();
            req.Settings = _settings;
            await GetBroker().PublishAndWaitAsync<AB_Save_Persona_Settings_Response>(req, DefaultTimeout);
        }

        /// <summary>모든 페르소나 이름 조회.</summary>
        public async Task<List<string>> GetPersonaNamesAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Persona_Names_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Persona_Names_Response>(req, DefaultTimeout);
            return resp.Names;
        }

        /// <summary>페르소나 이름 변경.</summary>
        public async Task<bool> RenameAsync(string _oldName, string _newName)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Rename_Persona_Request>();
            req.OldName = _oldName;
            req.NewName = _newName;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Rename_Persona_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>페르소나 삭제. 성공 시 다음 페르소나 이름 반환.</summary>
        public async Task<(bool Success, string? NextName, string? Error)> DeleteAsync(string _personaName)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Persona_Request>();
            req.PersonaName = _personaName;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Persona_Response>(req, DefaultTimeout);
            return (resp.Success, resp.NextPersonaName, resp.Error);
        }

        // ============================================================
        // Session CRUD
        // ============================================================

        public async Task<AB_Chat_Session_Model?> CreateSessionAsync(string _personaName, string _circuitName, int _turnShardSize = 0)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Create_Session_Request>();
            req.PersonaName = _personaName;
            req.CircuitName = _circuitName;
            req.TurnShardSize = _turnShardSize;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Create_Session_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        public async Task<AB_Chat_Session_Model?> GetSessionAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Session_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Session_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        public async Task<List<AB_Chat_Session_Model>> GetAllSessionsAsync(string _filter = "")
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Sessions_Request>();
            req.Filter = _filter;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Sessions_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        public async Task<List<AB_Chat_Session_Model>> GetSessionsAsync(string _circuitName, string _filter = "")
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Sessions_Request>();
            req.CircuitName = _circuitName;
            req.Filter = _filter;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Sessions_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        public async Task<int> GetSessionCountAsync(string _circuitName)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Session_Count_Request>();
            req.CircuitName = _circuitName;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Session_Count_Response>(req, DefaultTimeout);
            return resp.Count;
        }

        public async Task RenameSessionAsync(long _sessionId, string _newTitle)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Rename_Session_Request>();
            req.SessionId = _sessionId;
            req.NewTitle = _newTitle;
            await GetBroker().PublishAndWaitAsync<AB_Rename_Session_Response>(req, DefaultTimeout);
        }

        public async Task<AB_Chat_Session_Model?> CopySessionAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Copy_Session_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Copy_Session_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        public async Task MoveSessionAsync(long _sessionId, string _targetPersonaName)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Move_Session_Request>();
            req.SessionId = _sessionId;
            req.TargetPersonaName = _targetPersonaName;
            await GetBroker().PublishAndWaitAsync<AB_Move_Session_Response>(req, DefaultTimeout);
        }

        public async Task DeleteSessionAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Session_Request>();
            req.SessionId = _sessionId;
            await GetBroker().PublishAndWaitAsync<AB_Delete_Session_Response>(req, DefaultTimeout);
        }

        public async Task TouchSessionAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Touch_Session_Request>();
            req.SessionId = _sessionId;
            await GetBroker().PublishAndWaitAsync<AB_Touch_Session_Response>(req, DefaultTimeout);
        }

        public async Task<string?> UpdateSessionTitleFromFirstMessageAsync(long _sessionId, string _text)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Update_Session_Title_From_First_Message_Request>();
            req.SessionId = _sessionId;
            req.Text = _text;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Update_Session_Title_From_First_Message_Response>(req, DefaultTimeout);
            return resp.Title;
        }

        public async Task UpdateSessionCostAsync(long _sessionId, long _inputTokens, long _outputTokens, decimal _cost)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Update_Session_Cost_Request>();
            req.SessionId = _sessionId;
            req.InputTokens = _inputTokens;
            req.OutputTokens = _outputTokens;
            req.Cost = _cost;
            await GetBroker().PublishAndWaitAsync<AB_Update_Session_Cost_Response>(req, DefaultTimeout);
        }

        public async Task<(long inputTokens, long outputTokens, decimal cost)> GetSessionCostAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Session_Cost_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Session_Cost_Response>(req, DefaultTimeout);
            return (resp.InputTokens, resp.OutputTokens, resp.Cost);
        }

        // Phase 4.6 — Context_Record / Context_History 메서드 폐기.
        // 4 계층 storage ([[storage-layers]]) 의 Context_Storage / Node_Storage / Session_Storage 가 정본.

        /// <summary>세션 TurnShardSize 값만 갱신. 호출자가 사전에 데이터 wipe 를 책임짐.</summary>
        public async Task<bool> UpdateSessionTurnShardSizeAsync(long _sessionId, int _newSize)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Update_Session_Turn_Shard_Size_Request>();
            req.SessionId = _sessionId;
            req.NewTurnShardSize = _newSize;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Update_Session_Turn_Shard_Size_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>세션 CircuitName_ 만 갱신. chat 진행 중 circuit 교체용 (history 보존).</summary>
        public async Task<bool> UpdateSessionCircuitAsync(long _sessionId, string _circuitName)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Update_Session_Circuit_Request>();
            req.SessionId = _sessionId;
            req.CircuitName = _circuitName;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Update_Session_Circuit_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // MessageDataRef API (GetMessageDataRefsAsync / GetLatestMessageDataRefsAsync / PutMessageDataRefsAsync)
        // 은 Phase C 에서 폐기됨. context_records 단일 소스로 이관.

        // ============================================================
        // SavedImage
        // ============================================================

        public async Task AddSavedImageAsync(AB_Saved_Image_Model _img)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Saved_Image_Request>();
            req.Image = _img;
            await GetBroker().PublishAndWaitAsync<AB_Add_Saved_Image_Response>(req, DefaultTimeout);
        }

        // ============================================================
        // Vec (Chat Embedding)
        // ============================================================

        public async Task<List<AB_Vec_Chat_Hit>> SearchChatAsync(float[] _query, int _topK, string? _excludeSessionId = null)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Persona_Search_Chat_Request>();
            req.Query = _query;
            req.TopK = _topK;
            req.ExcludeSessionId = _excludeSessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Persona_Search_Chat_Response>(req, DefaultTimeout);
            return resp.Hits;
        }

        public async Task InsertChatEmbeddingAsync(long _sessionId, long _nodeId,
            int _turnIndex, int _refreshIndex, int _emissionOrder, float[] _embedding)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Persona_Insert_Chat_Embedding_Request>();
            req.SessionId = _sessionId;
            req.NodeId = _nodeId;
            req.TurnIndex = _turnIndex;
            req.RefreshIndex = _refreshIndex;
            req.EmissionOrder = _emissionOrder;
            req.Embedding = _embedding;
            await GetBroker().PublishAndWaitAsync<AB_Persona_Insert_Chat_Embedding_Response>(req, DefaultTimeout);
        }

        public async Task DeleteChatEmbeddingsBySessionAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Persona_Delete_Chat_Embeddings_By_Session_Request>();
            req.SessionId = _sessionId;
            await GetBroker().PublishAndWaitAsync<AB_Persona_Delete_Chat_Embeddings_By_Session_Response>(req, DefaultTimeout);
        }

        public async Task DeleteChatEmbeddingByRecordAsync(long _sessionId, long _nodeId,
            int _turnIndex, int _refreshIndex, int _emissionOrder)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Persona_Delete_Chat_Embedding_By_Record_Request>();
            req.SessionId = _sessionId;
            req.NodeId = _nodeId;
            req.TurnIndex = _turnIndex;
            req.RefreshIndex = _refreshIndex;
            req.EmissionOrder = _emissionOrder;
            await GetBroker().PublishAndWaitAsync<AB_Persona_Delete_Chat_Embedding_By_Record_Response>(req, DefaultTimeout);
        }

        public async Task<List<AB_Chat_Embedding_Info>> GetChatEmbeddingsBySessionAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Persona_Get_Chat_Embeddings_By_Session_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Persona_Get_Chat_Embeddings_By_Session_Response>(req, DefaultTimeout);
            return resp.Data;
        }
    }
}
