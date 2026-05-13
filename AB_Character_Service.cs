using ArtificialBuilder_EDP;
using ArtificialBuilder.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 캐릭터 비즈니스 로직 서비스.
    /// 캐릭터 CRUD, 관계, 장소, 내보내기/가져오기. UI 의존성 없음.
    /// </summary>
    public class AB_Character_Service : AB_Crud_Service<AB_Character_Model>
    {
        /// <summary>null=Circuit 템플릿 모드, 값=채팅 모드 (해당 세션 데이터만 조회/수정)</summary>
        private string? m_activeSessionId;
        public string? ActiveSessionId
        {
            get { return m_activeSessionId; }
            set { m_activeSessionId = value; }
        }

        // --- 캐릭터 CRUD ---

        public override async Task RefreshAsync()
        {
            // 게이트웨이 경유
            m_items = await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().GetAllCharactersAsync(string.IsNullOrEmpty(ActiveSessionId) ? 0L : long.Parse(ActiveSessionId));
            Emit(new Character_List_Changed { Characters_ = m_items });
        }

        /// <summary>신규 캐릭터 추가 후 목록 갱신 (게이트웨이 경유).</summary>
        public async Task<AB_Character_Model> AddAsync(string _name, int _sortOrder)
        {
            AB_Character_Model newChar = new AB_Character_Model
            {
                Name_ = _name,
                SortOrder_ = _sortOrder
            };
            await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().AddCharacterAsync(newChar, string.IsNullOrEmpty(ActiveSessionId) ? 0L : long.Parse(ActiveSessionId));
            await RefreshAsync();
            return newChar;
        }

        /// <summary>ID로 캐릭터 조회 (게이트웨이 경유).</summary>
        public async Task<AB_Character_Model?> GetAsync(string _id)
        {
            return await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().GetCharacterAsync(_id);
        }

        /// <summary>캐릭터 저장 후 목록 갱신 (게이트웨이 경유).</summary>
        public async Task SaveAsync(AB_Character_Model _character)
        {
            await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().SaveCharacterAsync(_character);
            await RefreshAsync();
        }

        /// <summary>캐릭터 삭제 후 목록 갱신 (게이트웨이 경유).</summary>
        public async Task DeleteAsync(AB_Character_Model _character)
        {
            await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().DeleteCharacterAsync(_character);
            await RefreshAsync();
        }

        // --- 관계 ---

        /// <summary>현재 컨텍스트의 관계 전체 조회.</summary>
        public async Task<List<AB_Character_Relationship_Model>> GetAllRelationshipsAsync()
        {
            return await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().GetAllRelationshipsAsync(string.IsNullOrEmpty(ActiveSessionId) ? 0L : long.Parse(ActiveSessionId));
        }

        // --- 장소 ---

        /// <summary>현재 컨텍스트의 장소 전체 조회.</summary>
        public async Task<List<AB_Location_Model>> GetAllLocationsAsync()
        {
            return await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().GetAllLocationsAsync(string.IsNullOrEmpty(ActiveSessionId) ? 0L : long.Parse(ActiveSessionId));
        }

        /// <summary>현재 컨텍스트의 장소 연결 전체 조회.</summary>
        public async Task<List<AB_Location_Connection_Model>> GetAllLocationConnectionsAsync()
        {
            return await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().GetAllLocationConnectionsAsync(string.IsNullOrEmpty(ActiveSessionId) ? 0L : long.Parse(ActiveSessionId));
        }

        // --- 내보내기/가져오기 ---

        /// <summary>캐릭터 데이터를 파일로 내보내기.</summary>
        public async Task ExportAsync(string _filePath)
        {
            var characters = await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().GetAllCharactersAsync();
            var relationships = await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().GetAllRelationshipsAsync();
            var data = new { Characters = characters, Relationships = relationships };
            string json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(_filePath, json);
        }

        /// <summary>파일에서 캐릭터 데이터를 가져와 목록 갱신.</summary>
        public async Task ImportAsync(string _filePath)
        {
            if (!System.IO.File.Exists(_filePath)) return;
            string json = await System.IO.File.ReadAllTextAsync(_filePath);
            // 파싱 로직은 원 스텁과 동일하게 로그만
            ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Debug("Character", $"캐릭터 가져오기: {_filePath}");
            await RefreshAsync();
        }
    }
}
