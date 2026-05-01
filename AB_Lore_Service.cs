using ArtificialBuilder.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 로어북 비즈니스 로직 서비스.
    /// 로어 CRUD + 임베딩. UI 의존성 없음.
    /// </summary>
    public class AB_Lore_Service : AB_Crud_Service<AB_Lore_Entry_Model>
    {
        // --- 로어 CRUD ---

        public override async Task RefreshAsync()
        {
            // 게이트웨이 경유
            m_items = await AB_Circuit_Db_Proxy.I.GetAllLoreEntriesAsync();
            Emit(new Lore_List_Changed { Entries_ = m_items });
        }

        /// <summary>이름으로 신규 로어 항목 추가 (게이트웨이 경유).</summary>
        public async Task<AB_Lore_Entry_Model> AddAsync(string _name)
        {
            AB_Lore_Entry_Model entry = new AB_Lore_Entry_Model
            {
                Name_ = _name
            };
            await AB_Circuit_Db_Proxy.I.AddLoreEntryAsync(entry);
            return entry;
        }

        /// <summary>ID로 로어 항목 조회 (게이트웨이 경유).</summary>
        public async Task<AB_Lore_Entry_Model?> GetAsync(string _id)
        {
            return await AB_Circuit_Db_Proxy.I.GetLoreEntryAsync(_id);
        }

        /// <summary>로어 항목 저장 후 목록 갱신 (게이트웨이 경유).</summary>
        public async Task SaveAsync(AB_Lore_Entry_Model _entry)
        {
            await AB_Circuit_Db_Proxy.I.SaveLoreEntryAsync(_entry);
            await RefreshAsync();
        }

        /// <summary>로어 항목 삭제 후 목록 갱신 (게이트웨이 경유).</summary>
        public async Task DeleteAsync(AB_Lore_Entry_Model _entry)
        {
            await AB_Circuit_Db_Proxy.I.DeleteLoreEntryAsync(_entry);
            await RefreshAsync();
        }
    }
}
