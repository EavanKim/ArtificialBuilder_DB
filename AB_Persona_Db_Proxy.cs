using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// Persona DB 프록시. 브로커 + AB_Persona_Db_Gateway 경유 호출.
    /// UI/서비스 계층에서 직접 DB 호출 대신 이 프록시를 사용.
    /// </summary>
    public class AB_Persona_Db_Proxy
    {
        private static AB_Persona_Db_Proxy? g_instance;
        /// <summary>전역 단일 인스턴스.</summary>
        public static AB_Persona_Db_Proxy I => g_instance ??= new AB_Persona_Db_Proxy();

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
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Active_Persona_Name_Response>(
                new AB_Get_Active_Persona_Name_Request(), DefaultTimeout);
            return resp.Name;
        }

        /// <summary>페르소나 설정 조회.</summary>
        public async Task<AB_Db_Result<AB_Persona_Settings_Model>> GetSettingsAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Persona_Settings_Response>(
                new AB_Get_Persona_Settings_Request(), DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Persona_Settings_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Persona_Settings_Model>.NotFound();
        }

        /// <summary>페르소나 설정 추가.</summary>
        public async Task AddSettingsAsync(AB_Persona_Settings_Model _settings)
        {
            await GetBroker().PublishAndWaitAsync<AB_Add_Persona_Settings_Response>(
                new AB_Add_Persona_Settings_Request { Settings = _settings }, DefaultTimeout);
        }

        /// <summary>페르소나 설정 저장.</summary>
        public async Task SaveSettingsAsync(AB_Persona_Settings_Model _settings)
        {
            await GetBroker().PublishAndWaitAsync<AB_Save_Persona_Settings_Response>(
                new AB_Save_Persona_Settings_Request { Settings = _settings }, DefaultTimeout);
        }

        /// <summary>모든 페르소나 이름 조회.</summary>
        public async Task<List<string>> GetPersonaNamesAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Persona_Names_Response>(
                new AB_Get_Persona_Names_Request(), DefaultTimeout);
            return resp.Names;
        }

        /// <summary>페르소나 이름 변경.</summary>
        public async Task<bool> RenameAsync(string _oldName, string _newName)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Rename_Persona_Response>(
                new AB_Rename_Persona_Request { OldName = _oldName, NewName = _newName }, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>페르소나 삭제. 성공 시 다음 페르소나 이름 반환.</summary>
        public async Task<(bool Success, string? NextName, string? Error)> DeleteAsync(string _personaName)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Persona_Response>(
                new AB_Delete_Persona_Request { PersonaName = _personaName }, DefaultTimeout);
            return (resp.Success, resp.NextPersonaName, resp.Error);
        }

        // ============================================================
        // Session CRUD
        // ============================================================

        public async Task<AB_Chat_Session_Model?> CreateSessionAsync(string _personaName, string _circuitName, int _turnShardSize = 0)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Create_Session_Response>(
                new AB_Create_Session_Request
                {
                    PersonaName = _personaName,
                    CircuitName = _circuitName,
                    TurnShardSize = _turnShardSize
                }, DefaultTimeout);
            return resp.Data;
        }

        public async Task<AB_Chat_Session_Model?> GetSessionAsync(string _sessionId)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Session_Response>(
                new AB_Get_Session_Request { SessionId = long.Parse(_sessionId) }, DefaultTimeout);
            return resp.Data;
        }

        public async Task<List<AB_Chat_Session_Model>> GetAllSessionsAsync(string _filter = "")
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Sessions_Response>(
                new AB_Get_All_Sessions_Request { Filter = _filter }, DefaultTimeout);
            return resp.Data;
        }

        public async Task<List<AB_Chat_Session_Model>> GetSessionsAsync(string _circuitName, string _filter = "")
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Sessions_Response>(
                new AB_Get_Sessions_Request { CircuitName = _circuitName, Filter = _filter }, DefaultTimeout);
            return resp.Data;
        }

        public async Task<int> GetSessionCountAsync(string _circuitName)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Session_Count_Response>(
                new AB_Get_Session_Count_Request { CircuitName = _circuitName }, DefaultTimeout);
            return resp.Count;
        }

        public async Task RenameSessionAsync(string _sessionId, string _newTitle)
        {
            await GetBroker().PublishAndWaitAsync<AB_Rename_Session_Response>(
                new AB_Rename_Session_Request { SessionId = long.Parse(_sessionId), NewTitle = _newTitle }, DefaultTimeout);
        }

        public async Task<AB_Chat_Session_Model?> CopySessionAsync(string _sessionId)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Copy_Session_Response>(
                new AB_Copy_Session_Request { SessionId = long.Parse(_sessionId) }, DefaultTimeout);
            return resp.Data;
        }

        public async Task MoveSessionAsync(string _sessionId, string _targetPersonaName)
        {
            await GetBroker().PublishAndWaitAsync<AB_Move_Session_Response>(
                new AB_Move_Session_Request { SessionId = long.Parse(_sessionId), TargetPersonaName = _targetPersonaName }, DefaultTimeout);
        }

        public async Task DeleteSessionAsync(string _sessionId)
        {
            await GetBroker().PublishAndWaitAsync<AB_Delete_Session_Response>(
                new AB_Delete_Session_Request { SessionId = long.Parse(_sessionId) }, DefaultTimeout);
        }

        public async Task TouchSessionAsync(string _sessionId)
        {
            await GetBroker().PublishAndWaitAsync<AB_Touch_Session_Response>(
                new AB_Touch_Session_Request { SessionId = long.Parse(_sessionId) }, DefaultTimeout);
        }

        public async Task<string?> UpdateSessionTitleFromFirstMessageAsync(string _sessionId, string _text)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Update_Session_Title_From_First_Message_Response>(
                new AB_Update_Session_Title_From_First_Message_Request { SessionId = long.Parse(_sessionId), Text = _text }, DefaultTimeout);
            return resp.Title;
        }

        public async Task UpdateSessionCostAsync(string _sessionId, long _inputTokens, long _outputTokens, decimal _cost)
        {
            await GetBroker().PublishAndWaitAsync<AB_Update_Session_Cost_Response>(
                new AB_Update_Session_Cost_Request
                {
                    SessionId = long.Parse(_sessionId),
                    InputTokens = _inputTokens,
                    OutputTokens = _outputTokens,
                    Cost = _cost
                }, DefaultTimeout);
        }

        public async Task<(long inputTokens, long outputTokens, decimal cost)> GetSessionCostAsync(string _sessionId)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Session_Cost_Response>(
                new AB_Get_Session_Cost_Request { SessionId = long.Parse(_sessionId) }, DefaultTimeout);
            return (resp.InputTokens, resp.OutputTokens, resp.Cost);
        }

        // Phase 4.6 — Context_Record / Context_History 메서드 폐기.
        // 4 계층 storage ([[storage-layers]]) 의 Context_Storage / Node_Storage / Session_Storage 가 정본.

        /// <summary>세션 TurnShardSize 값만 갱신. 호출자가 사전에 데이터 wipe 를 책임짐.</summary>
        public async Task<bool> UpdateSessionTurnShardSizeAsync(string _sessionId, int _newSize)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Update_Session_Turn_Shard_Size_Response>(
                new AB_Update_Session_Turn_Shard_Size_Request
                {
                    SessionId = long.Parse(_sessionId),
                    NewTurnShardSize = _newSize
                }, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>세션 CircuitName_ 만 갱신. chat 진행 중 circuit 교체용 (history 보존).</summary>
        public async Task<bool> UpdateSessionCircuitAsync(string _sessionId, string _circuitName)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Update_Session_Circuit_Response>(
                new AB_Update_Session_Circuit_Request
                {
                    SessionId = long.Parse(_sessionId),
                    CircuitName = _circuitName
                }, DefaultTimeout);
            return resp.Success;
        }

        // MessageDataRef API (GetMessageDataRefsAsync / GetLatestMessageDataRefsAsync / PutMessageDataRefsAsync)
        // 은 Phase C 에서 폐기됨. context_records 단일 소스로 이관.

        // ============================================================
        // SavedImage
        // ============================================================

        public async Task AddSavedImageAsync(AB_Saved_Image_Model _img)
        {
            await GetBroker().PublishAndWaitAsync<AB_Add_Saved_Image_Response>(
                new AB_Add_Saved_Image_Request { Image = _img }, DefaultTimeout);
        }

        // ============================================================
        // Vec (Chat Embedding)
        // ============================================================

        public async Task<List<AB_Vec_Chat_Hit>> SearchChatAsync(float[] _query, int _topK, string? _excludeSessionId = null)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Persona_Search_Chat_Response>(
                new AB_Persona_Search_Chat_Request { Query = _query, TopK = _topK, ExcludeSessionId = _excludeSessionId }, DefaultTimeout);
            return resp.Hits;
        }

        public async Task InsertChatEmbeddingAsync(string _sessionId, string _nodeId,
            int _turnIndex, int _refreshIndex, int _emissionOrder, float[] _embedding)
        {
            await GetBroker().PublishAndWaitAsync<AB_Persona_Insert_Chat_Embedding_Response>(
                new AB_Persona_Insert_Chat_Embedding_Request
                {
                    SessionId = long.Parse(_sessionId),
                    NodeId = _nodeId,
                    TurnIndex = _turnIndex,
                    RefreshIndex = _refreshIndex,
                    EmissionOrder = _emissionOrder,
                    Embedding = _embedding
                }, DefaultTimeout);
        }

        public async Task DeleteChatEmbeddingsBySessionAsync(string _sessionId)
        {
            await GetBroker().PublishAndWaitAsync<AB_Persona_Delete_Chat_Embeddings_By_Session_Response>(
                new AB_Persona_Delete_Chat_Embeddings_By_Session_Request { SessionId = long.Parse(_sessionId) }, DefaultTimeout);
        }

        public async Task DeleteChatEmbeddingByRecordAsync(string _sessionId, string _nodeId,
            int _turnIndex, int _refreshIndex, int _emissionOrder)
        {
            await GetBroker().PublishAndWaitAsync<AB_Persona_Delete_Chat_Embedding_By_Record_Response>(
                new AB_Persona_Delete_Chat_Embedding_By_Record_Request
                {
                    SessionId = long.Parse(_sessionId),
                    NodeId = _nodeId,
                    TurnIndex = _turnIndex,
                    RefreshIndex = _refreshIndex,
                    EmissionOrder = _emissionOrder
                }, DefaultTimeout);
        }

        public async Task<List<AB_Chat_Embedding_Info>> GetChatEmbeddingsBySessionAsync(string _sessionId)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Persona_Get_Chat_Embeddings_By_Session_Response>(
                new AB_Persona_Get_Chat_Embeddings_By_Session_Request { SessionId = long.Parse(_sessionId) }, DefaultTimeout);
            return resp.Data;
        }
    }
}
