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
    /// 기존 호출처 호환을 위한 얇은 프록시. 메시지 브로커 + AB_Circuit_Db_Gateway 경유 호출.
    /// AB_Db_Manager.Instance.Circuit.X() 같은 직접 호출과 동일한 시그니처를 노출하되
    /// 내부적으로 publish/await 사이클을 거쳐 트랜잭셔널 보장 + 향후 컨텍스트 라우팅 진입점.
    ///
    /// 사용:
    ///   var proxy = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>();
    ///   var chars = await proxy.GetAllCharactersAsync();
    /// </summary>
    public class AB_Circuit_Db_Proxy : ArtificialBuilder_EDP.Core.AB_Object
    {
        /// <summary>요청-응답 기본 타임아웃.</summary>
        private TimeSpan m_defaultTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan DefaultTimeout
        {
            get { return m_defaultTimeout; }
            set { m_defaultTimeout = value; }
        }

        private IAB_Message_Broker GetBroker()
            => ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();

        // --- 5개 시범 메서드 ---

        /// <summary>활성 circuit의 캐릭터 목록.</summary>
        public async Task<List<AB_Character_Model>> GetAllCharactersAsync(long _sessionId = 0L)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Characters_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Characters_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>ID로 단일 캐릭터 조회.</summary>
        public async Task<AB_Character_Model?> GetCharacterAsync(string _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Character_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Character_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        /// <summary>캐릭터 추가.</summary>
        public async Task<bool> AddCharacterAsync(AB_Character_Model _character, long _sessionId = 0L)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Character_Request>();
            req.Character = _character;
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Character_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>캐릭터 갱신.</summary>
        public async Task<bool> SaveCharacterAsync(AB_Character_Model _character)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Character_Request>();
            req.Character = _character;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Character_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>캐릭터 삭제.</summary>
        public async Task<bool> DeleteCharacterAsync(AB_Character_Model _character)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Character_Request>();
            req.Character = _character;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Character_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>관계 전체.</summary>
        public async Task<List<AB_Character_Relationship_Model>> GetAllRelationshipsAsync(long _sessionId = 0L)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Relationships_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Relationships_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>장소 전체.</summary>
        public async Task<List<AB_Location_Model>> GetAllLocationsAsync(long _sessionId = 0L)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Locations_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Locations_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>장소 연결 전체.</summary>
        public async Task<List<AB_Location_Connection_Model>> GetAllLocationConnectionsAsync(long _sessionId = 0L)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Location_Connections_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Location_Connections_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>응답 윈도우 전체.</summary>
        public async Task<List<AB_Response_Ui_Window_Model>> GetAllWindowsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Windows_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Windows_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>응답 윈도우 단건 조회.</summary>
        public async Task<AB_Db_Result<AB_Response_Ui_Window_Model>> GetWindowAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Window_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Window_Response>(req, DefaultTimeout);
            return resp.IsOk && resp.Data != null
                ? AB_Db_Result<AB_Response_Ui_Window_Model>.Ok(resp.Data)
                : AB_Db_Result<AB_Response_Ui_Window_Model>.NotFound();
        }

        /// <summary>응답 윈도우 추가.</summary>
        public async Task<bool> AddWindowAsync(AB_Response_Ui_Window_Model _window)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Window_Request>();
            req.Window = _window;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Window_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>응답 윈도우 갱신.</summary>
        public async Task<bool> SaveWindowAsync(AB_Response_Ui_Window_Model _window)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Window_Request>();
            req.Window = _window;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Window_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>응답 윈도우 삭제.</summary>
        public async Task<bool> DeleteWindowAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Window_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Window_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>특정 윈도우의 컴포넌트 전체 조회.</summary>
        public async Task<List<AB_Response_Ui_Component_Model>> GetWindowComponentsAsync(long _windowId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Window_Components_Request>();
            req.WindowId = _windowId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Window_Components_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>전 윈도우의 컴포넌트 통합 조회.</summary>
        public async Task<List<AB_Response_Ui_Component_Model>> GetAllWindowComponentsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Window_Components_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Window_Components_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>윈도우 컴포넌트 추가.</summary>
        public async Task<bool> AddWindowComponentAsync(AB_Response_Ui_Component_Model _component)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Window_Component_Request>();
            req.Component = _component;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Window_Component_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>윈도우 컴포넌트 갱신.</summary>
        public async Task<bool> SaveWindowComponentAsync(AB_Response_Ui_Component_Model _component)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Window_Component_Request>();
            req.Component = _component;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Window_Component_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>윈도우 컴포넌트 삭제.</summary>
        public async Task<bool> DeleteWindowComponentAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Window_Component_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Window_Component_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>윈도우의 전 컴포넌트 cascade 삭제.</summary>
        public async Task<int> DeleteWindowComponentsByWindowAsync(long _windowId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Window_Components_By_Window_Request>();
            req.WindowId = _windowId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Window_Components_By_Window_Response>(req, DefaultTimeout);
            return resp.DeletedCount;
        }

        /// <summary>세션 캐릭터 데이터 전체.</summary>
        public async Task<List<AB_Character_Data_Model>> GetAllSessionDataAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Session_Data_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Session_Data_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>데이터 카테고리 전체.</summary>
        public async Task<List<AB_Data_Category_Model>> GetAllDataCategoriesAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Data_Categories_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Data_Categories_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>세션 데이터 일괄 삭제.</summary>
        public async Task<bool> DeleteSessionDataAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Session_Data_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Session_Data_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>특정 메시지 캐릭터 데이터 삭제.</summary>
        public async Task<bool> DeleteCharacterDataByMessageAsync(long _messageId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Character_Data_By_Message_Request>();
            req.MessageId = _messageId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Character_Data_By_Message_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>지정 메시지 이후 캐릭터 데이터 삭제.</summary>
        public async Task<bool> DeleteCharacterDataFromMessageAsync(long _fromMessageId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Character_Data_From_Message_Request>();
            req.FromMessageId = _fromMessageId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Character_Data_From_Message_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>Circuit 데이터를 세션으로 복사.</summary>
        public async Task<bool> CopyCircuitDataToSessionAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Copy_Circuit_Data_To_Session_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Copy_Circuit_Data_To_Session_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>카테고리별 캐릭터 데이터 조회.</summary>
        public async Task<List<AB_Character_Data_Model>> GetCharacterDataByCategoryAsync(
            string _characterId, long _sessionId, string _categoryId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Character_Data_By_Category_Request>();
            req.CharacterId = _characterId;
            req.SessionId = _sessionId;
            req.CategoryId = _categoryId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Character_Data_By_Category_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>캐릭터 데이터 upsert.</summary>
        public async Task<bool> UpsertCharacterDataAsync(
            string _characterId, long _sessionId, string _categoryId,
            string _fieldName, string? _fieldValue, string? _narrative,
            string _source, long? _messageId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Upsert_Character_Data_Request>();
            req.CharacterId = _characterId;
            req.SessionId = _sessionId;
            req.CategoryId = _categoryId;
            req.FieldName = _fieldName;
            req.FieldValue = _fieldValue;
            req.Narrative = _narrative;
            req.Source = _source;
            req.MessageId = _messageId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Upsert_Character_Data_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>캐릭터 데이터 단건 삭제.</summary>
        public async Task<bool> DeleteCharacterDataAsync(string _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Character_Data_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Character_Data_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>데이터 카테고리 추가.</summary>
        public async Task<bool> AddDataCategoryAsync(AB_Data_Category_Model _category)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Data_Category_Request>();
            req.Category = _category;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Data_Category_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>패턴 전체.</summary>
        public async Task<List<AB_Pattern_Config_Model>> GetAllPatternsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Patterns_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Patterns_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>관계 색상 전체.</summary>
        public async Task<List<AB_Relation_Color_Model>> GetAllRelationColorsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Relation_Colors_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Relation_Colors_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>관계 색상 추가.</summary>
        public async Task<bool> AddRelationColorAsync(AB_Relation_Color_Model _color)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Relation_Color_Request>();
            req.Color = _color;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Relation_Color_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>관계 색상 갱신.</summary>
        public async Task<bool> SaveRelationColorAsync(AB_Relation_Color_Model _color)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Relation_Color_Request>();
            req.Color = _color;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Relation_Color_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>관계 색상 삭제.</summary>
        public async Task<bool> DeleteRelationColorAsync(AB_Relation_Color_Model _color)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Relation_Color_Request>();
            req.Color = _color;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Relation_Color_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>활성 circuit의 lore 목록.</summary>
        public async Task<List<AB_Lore_Entry_Model>> GetAllLoreEntriesAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Lore_Entries_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Lore_Entries_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>ID로 단일 lore 조회 (없으면 null).</summary>
        public async Task<AB_Lore_Entry_Model?> GetLoreEntryAsync(string _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Lore_Entry_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Lore_Entry_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        /// <summary>lore 추가.</summary>
        public async Task<bool> AddLoreEntryAsync(AB_Lore_Entry_Model _entry)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Lore_Entry_Request>();
            req.Entry = _entry;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Lore_Entry_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>lore 갱신.</summary>
        public async Task<bool> SaveLoreEntryAsync(AB_Lore_Entry_Model _entry)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Lore_Entry_Request>();
            req.Entry = _entry;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Lore_Entry_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>lore 삭제.</summary>
        public async Task<bool> DeleteLoreEntryAsync(AB_Lore_Entry_Model _entry)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Lore_Entry_Request>();
            req.Entry = _entry;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Lore_Entry_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>텍스트 기반 lore 매칭 검색.</summary>
        public async Task<List<AB_Lore_Entry_Model>> FindMatchingLoreAsync(string _text)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Find_Matching_Lore_Request>();
            req.Text = _text;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Find_Matching_Lore_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>활성 circuit settings (AB_Db_Result 호환 형태로 반환).</summary>
        public async Task<AB_Db_Result<AB_Circuit_Settings_Model>> GetSettingsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Circuit_Settings_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Circuit_Settings_Response>(req, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Circuit_Settings_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Circuit_Settings_Model>.NotFound();
        }

        /// <summary>활성 circuit settings 갱신 (성공 시 true).</summary>
        public async Task<bool> SaveSettingsAsync(AB_Circuit_Settings_Model _settings)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Settings_Request>();
            req.Settings = _settings;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Settings_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>활성 circuit에 settings 신규 삽입 (성공 시 true).</summary>
        public async Task<bool> AddSettingsAsync(AB_Circuit_Settings_Model _settings)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Settings_Request>();
            req.Settings = _settings;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Settings_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>전체 에셋 (Data_ 포함, 무거움).</summary>
        public async Task<List<AB_Circuit_Asset_Model>> GetAllAssetsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Assets_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Assets_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>특정 에셋의 바이너리 데이터.</summary>
        public async Task<byte[]?> GetAssetDataAsync(string _assetId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Asset_Data_Request>();
            req.AssetId = _assetId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Asset_Data_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        /// <summary>ID로 단일 에셋.</summary>
        public async Task<AB_Circuit_Asset_Model?> GetAssetAsync(string _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Asset_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Asset_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        /// <summary>이름으로 단일 에셋.</summary>
        public async Task<AB_Circuit_Asset_Model?> GetAssetByNameAsync(string _name)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Asset_By_Name_Request>();
            req.Name = _name;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Asset_By_Name_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        /// <summary>단일 에셋 추가.</summary>
        public async Task<bool> AddAssetAsync(AB_Circuit_Asset_Model _asset)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Asset_Request>();
            req.Asset = _asset;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Asset_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>여러 에셋 일괄 추가.</summary>
        public async Task<bool> AddAssetsAsync(List<AB_Circuit_Asset_Model> _assets)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Assets_Request>();
            req.Assets = _assets;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Assets_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>에셋 삭제.</summary>
        public async Task<bool> DeleteAssetAsync(AB_Circuit_Asset_Model _asset)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Asset_Request>();
            req.Asset = _asset;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Asset_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>경로 prefix로 에셋 검색.</summary>
        public async Task<List<AB_Circuit_Asset_Model>> FindAssetsByPathPrefixAsync(string _prefix)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Find_Assets_By_Path_Prefix_Request>();
            req.Prefix = _prefix;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Find_Assets_By_Path_Prefix_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>활성 circuit의 에셋 메타데이터.</summary>
        public async Task<List<AB_Circuit_Asset_Model>> GetAllAssetMetadataAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Asset_Metadata_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Asset_Metadata_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>활성 circuit의 UI 템플릿 목록.</summary>
        public async Task<List<AB_Response_Ui_Template_Model>> GetAllUiTemplatesAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Ui_Templates_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Ui_Templates_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>UI 템플릿 추가.</summary>
        public async Task<bool> AddUiTemplateAsync(AB_Response_Ui_Template_Model _template)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Ui_Template_Request>();
            req.Template = _template;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Ui_Template_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>UI 템플릿 저장.</summary>
        public async Task<bool> SaveUiTemplateAsync(AB_Response_Ui_Template_Model _template)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Ui_Template_Request>();
            req.Template = _template;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Ui_Template_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>UI 템플릿 삭제.</summary>
        public async Task<bool> DeleteUiTemplateAsync(AB_Response_Ui_Template_Model _template)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Ui_Template_Request>();
            req.Template = _template;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Ui_Template_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>활성 UI 템플릿 설정.</summary>
        public async Task<bool> SetActiveUiTemplateAsync(string _templateId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Set_Active_Ui_Template_Request>();
            req.TemplateId = _templateId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Set_Active_Ui_Template_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>활성 UI 템플릿 조회.</summary>
        public async Task<AB_Response_Ui_Template_Model?> GetActiveUiTemplateAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Active_Ui_Template_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Active_Ui_Template_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        // ====================================================================
        // H. Vec — 임베딩/벡터 검색
        // ====================================================================

        /// <summary>벡터 저장소 초기화 여부.</summary>
        public async Task<bool> IsVecInitializedAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Is_Vec_Initialized_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Is_Vec_Initialized_Response>(req, DefaultTimeout);
            return resp.Initialized;
        }

        /// <summary>vec 전체 행 수.</summary>
        public async Task<int> GetVecTotalRowCountAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Vec_Total_Row_Count_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Vec_Total_Row_Count_Response>(req, DefaultTimeout);
            return resp.Count;
        }

        /// <summary>vec 전체 데이터 삭제.</summary>
        public async Task<bool> ClearAllVecAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Clear_All_Vec_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Clear_All_Vec_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>로어 임베딩 검색.</summary>
        public async Task<List<AB_Vec_Hit>> SearchLoreVecAsync(float[] _query, int _topK)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Search_Lore_Vec_Request>();
            req.Query = _query;
            req.TopK = _topK;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Search_Lore_Vec_Response>(req, DefaultTimeout);
            return resp.Hits ?? new();
        }

        /// <summary>채팅 임베딩 검색.</summary>
        public async Task<List<AB_Vec_Chat_Hit>> SearchChatVecAsync(float[] _query, int _topK, string? _excludeSessionId = null)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Search_Chat_Vec_Request>();
            req.Query = _query;
            req.TopK = _topK;
            req.ExcludeSessionId = _excludeSessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Search_Chat_Vec_Response>(req, DefaultTimeout);
            return resp.Hits ?? new();
        }

        /// <summary>CData 임베딩 검색.</summary>
        public async Task<List<AB_Vec_Hit>> SearchCDataVecAsync(float[] _query, int _topK = 10)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Search_CData_Vec_Request>();
            req.Query = _query;
            req.TopK = _topK;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Search_CData_Vec_Response>(req, DefaultTimeout);
            return resp.Hits ?? new();
        }

        /// <summary>로어 임베딩 삽입.</summary>
        public async Task<bool> InsertLoreEmbeddingAsync(string _loreId, float[] _embedding)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Insert_Lore_Embedding_Request>();
            req.LoreId = _loreId;
            req.Embedding = _embedding;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Insert_Lore_Embedding_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>로어 임베딩 삭제.</summary>
        public async Task<bool> DeleteLoreEmbeddingAsync(string _loreId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Lore_Embedding_Request>();
            req.LoreId = _loreId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Lore_Embedding_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>채팅 임베딩 삽입. 키 튜플은 context_records 와 동일.</summary>
        public async Task<bool> InsertChatEmbeddingAsync(long _sessionId, long _nodeId,
            int _turnIndex, int _refreshIndex, int _emissionOrder, float[] _embedding)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Insert_Chat_Embedding_Request>();
            req.SessionId = _sessionId;
            req.NodeId = _nodeId;
            req.TurnIndex = _turnIndex;
            req.RefreshIndex = _refreshIndex;
            req.EmissionOrder = _emissionOrder;
            req.Embedding = _embedding;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Insert_Chat_Embedding_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>세션 채팅 임베딩 일괄 삭제.</summary>
        public async Task<bool> DeleteChatEmbeddingsBySessionAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Chat_Embeddings_By_Session_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Chat_Embeddings_By_Session_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>특정 컨텍스트 레코드의 채팅 임베딩 삭제.</summary>
        public async Task<bool> DeleteChatEmbeddingByRecordAsync(long _sessionId, long _nodeId,
            int _turnIndex, int _refreshIndex, int _emissionOrder)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Chat_Embedding_By_Record_Request>();
            req.SessionId = _sessionId;
            req.NodeId = _nodeId;
            req.TurnIndex = _turnIndex;
            req.RefreshIndex = _refreshIndex;
            req.EmissionOrder = _emissionOrder;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Chat_Embedding_By_Record_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>세션 임베딩 메타 목록.</summary>
        public async Task<List<AB_Chat_Embedding_Info>> GetChatEmbeddingsBySessionAsync(long _sessionId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Chat_Embeddings_By_Session_Request>();
            req.SessionId = _sessionId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Chat_Embeddings_By_Session_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>CData 임베딩 삽입.</summary>
        public async Task<bool> InsertCDataEmbeddingAsync(string _cdataId, float[] _embedding)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Insert_CData_Embedding_Request>();
            req.CDataId = _cdataId;
            req.Embedding = _embedding;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Insert_CData_Embedding_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>Vec 파일 열기.</summary>
        public async Task<bool> OpenVecFileAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Open_Vec_File_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Open_Vec_File_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>Vec 파일 이름 변경.</summary>
        public async Task<bool> RenameVecFileAsync(string _oldName, string _newName)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Rename_Vec_File_Request>();
            req.OldName = _oldName;
            req.NewName = _newName;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Rename_Vec_File_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        /// <summary>Circuit 파일 삭제 — Gateway 가 핸들 close 후 .circuit + .vec 정리. [[db-access]] 단일 창구.</summary>
        public async Task<bool> DeleteCircuitFileAsync(string _name)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Circuit_File_Request>();
            req.Name = _name;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Circuit_File_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // ====================================================================
        // circuit-home-logic-graph-runtime-db-proxy sub 4 — v2 슬롯 / hosted_logic_slot_value
        // ====================================================================

        public async Task<List<AB_Circuit_Input_Slot_Model>> GetAllInputSlotsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Circuit_Input_Slots_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Circuit_Input_Slots_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddInputSlotAsync(AB_Circuit_Input_Slot_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Circuit_Input_Slot_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Circuit_Input_Slot_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> SaveInputSlotAsync(AB_Circuit_Input_Slot_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Circuit_Input_Slot_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Circuit_Input_Slot_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> RemoveInputSlotAsync(AB_Slot_Id _slotId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Remove_Circuit_Input_Slot_Request>();
            req.Slot_Id = _slotId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Remove_Circuit_Input_Slot_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<List<AB_Circuit_Output_Slot_Model>> GetAllOutputSlotsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Circuit_Output_Slots_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Circuit_Output_Slots_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddOutputSlotAsync(AB_Circuit_Output_Slot_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Circuit_Output_Slot_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Circuit_Output_Slot_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> SaveOutputSlotAsync(AB_Circuit_Output_Slot_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Circuit_Output_Slot_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Save_Circuit_Output_Slot_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> RemoveOutputSlotAsync(AB_Slot_Id _slotId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Remove_Circuit_Output_Slot_Request>();
            req.Slot_Id = _slotId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Remove_Circuit_Output_Slot_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<List<AB_Circuit_Hosted_Logic_Slot_Value_Model>> GetAllHostedLogicSlotValuesAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Hosted_Logic_Slot_Values_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Hosted_Logic_Slot_Values_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<AB_Circuit_Hosted_Logic_Slot_Value_Model?> GetHostedLogicSlotValueAsync(AB_Logic_Id _logicId, AB_Slot_Id _slotId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Hosted_Logic_Slot_Value_Request>();
            req.Logic_Id = _logicId;
            req.Slot_Id = _slotId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Hosted_Logic_Slot_Value_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        public async Task<bool> SetHostedLogicSlotValueAsync(AB_Circuit_Hosted_Logic_Slot_Value_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Set_Hosted_Logic_Slot_Value_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Set_Hosted_Logic_Slot_Value_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<List<AB_Circuit_Hosted_Logic_Model>> GetHostedLogicsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Hosted_Logics_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Hosted_Logics_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> AddHostedLogicAsync(AB_Circuit_Hosted_Logic_Model _item)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Hosted_Logic_Request>();
            req.Item = _item;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_Hosted_Logic_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<bool> RemoveHostedLogicAsync(AB_Logic_Id _hostedLogicId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Remove_Hosted_Logic_Request>();
            req.Hosted_Logic_Id = _hostedLogicId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Remove_Hosted_Logic_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // --- circuit-as-node-graph sub 2 (2026-05-07) — hosted_logic 슬롯 매핑 + UI 매핑 (atomic replace) ---
        // 정본: docs/architecture/3-pipeline/circuit-as-node-graph.md / data-by-blackboard-key-only.md

        public async Task<List<AB_Hosted_Slot_Mapping_Model>> GetHostedLogicSlotMappingsAsync(AB_Logic_Id _hostedLogicId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Hosted_Logic_Slot_Mappings_Request>();
            req.Hosted_Logic_Id = _hostedLogicId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Hosted_Logic_Slot_Mappings_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> ReplaceHostedLogicSlotMappingsAsync(AB_Logic_Id _hostedLogicId, List<AB_Hosted_Slot_Mapping_Model> _items)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Replace_Hosted_Logic_Slot_Mappings_Request>();
            req.Hosted_Logic_Id = _hostedLogicId;
            req.Items = _items;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Replace_Hosted_Logic_Slot_Mappings_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        public async Task<List<AB_Hosted_Ui_Mapping_Model>> GetHostedLogicUiMappingsAsync(AB_Logic_Id _hostedLogicId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Hosted_Logic_Ui_Mappings_Request>();
            req.Hosted_Logic_Id = _hostedLogicId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Hosted_Logic_Ui_Mappings_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        public async Task<bool> ReplaceHostedLogicUiMappingsAsync(AB_Logic_Id _hostedLogicId, List<AB_Hosted_Ui_Mapping_Model> _items)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Replace_Hosted_Logic_Ui_Mappings_Request>();
            req.Hosted_Logic_Id = _hostedLogicId;
            req.Items = _items;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Replace_Hosted_Logic_Ui_Mappings_Response>(req, DefaultTimeout);
            return resp.Success;
        }
    }
}
