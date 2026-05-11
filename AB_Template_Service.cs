using ArtificialBuilder_EDP;
using ArtificialBuilder.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 템플릿 비즈니스 로직 서비스.
    /// 글로벌 템플릿 (AppDb) + Circuit 템플릿 (CircuitDb) CRUD. UI 의존성 없음.
    /// </summary>
    public class AB_Template_Service : AB_Service
    {
        // ===== 글로벌 템플릿 (AppDb) =====

        /// <summary>글로벌 템플릿 전체 조회.</summary>
        public async Task<List<AB_Ui_Template_Model>> GetAllGlobalAsync()
        {
            return await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().GetAllUiTemplatesAsync();
        }

        /// <summary>글로벌 템플릿 ID로 조회.</summary>
        public async Task<AB_Db_Result<AB_Ui_Template_Model>> GetGlobalByIdAsync(string _id)
        {
            return await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().GetUiTemplateByIdAsync(_id);
        }

        /// <summary>글로벌 템플릿 추가.</summary>
        public async Task AddGlobalAsync(AB_Ui_Template_Model _template)
        {
            await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().AddUiTemplateAsync(_template);
            Emit(new Template_List_Changed());
        }

        /// <summary>글로벌 템플릿 수정.</summary>
        public async Task UpdateGlobalAsync(AB_Ui_Template_Model _template)
        {
            await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().UpdateUiTemplateAsync(_template);
            Emit(new Template_List_Changed());
        }

        /// <summary>글로벌 템플릿 삭제.</summary>
        public async Task DeleteGlobalAsync(AB_Ui_Template_Model _template)
        {
            await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().DeleteUiTemplateAsync(_template);
            Emit(new Template_List_Changed());
        }

        // ===== Circuit 템플릿 (CircuitDb) =====

        /// <summary>Circuit 템플릿 전체 조회 (게이트웨이 경유).</summary>
        public async Task<List<AB_Response_Ui_Template_Model>> GetAllCircuitAsync()
        {
            return await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().GetAllUiTemplatesAsync();
        }

        /// <summary>Circuit 템플릿 추가.</summary>
        public async Task AddCircuitAsync(AB_Response_Ui_Template_Model _template)
        {
            await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().AddUiTemplateAsync(_template);
            Emit(new Template_List_Changed());
        }

        /// <summary>Circuit 템플릿 저장.</summary>
        public async Task SaveCircuitAsync(AB_Response_Ui_Template_Model _template)
        {
            await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().SaveUiTemplateAsync(_template);
            Emit(new Template_List_Changed());
        }

        /// <summary>Circuit 템플릿 삭제. 마지막 템플릿이면 false 반환.</summary>
        public async Task<bool> DeleteCircuitAsync(AB_Response_Ui_Template_Model _template)
        {
            return await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().DeleteUiTemplateAsync(_template);
        }

        /// <summary>Circuit 활성 템플릿 설정.</summary>
        public async Task SetActiveCircuitAsync(string _templateId)
        {
            await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().SetActiveUiTemplateAsync(_templateId);
        }

        /// <summary>Circuit 활성 템플릿 조회.</summary>
        public async Task<AB_Response_Ui_Template_Model?> GetActiveCircuitAsync()
        {
            return await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().GetActiveUiTemplateAsync();
        }

        /// <summary>글로벌 → Circuit으로 템플릿 가져오기.</summary>
        public async Task ImportFromGlobalAsync(string _globalTemplateId, int _sortOrder)
        {
            var sourceResult = await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().GetUiTemplateByIdAsync(_globalTemplateId);
            if (sourceResult.IsOk)
            {
                var source = sourceResult.Data;
                AB_Response_Ui_Template_Model imported = new AB_Response_Ui_Template_Model
                {
                    Name_ = source.Name_,
                    DisplayMode_ = source.DisplayMode_,
                    XmlContent_ = source.XmlContent_,
                    IsActive_ = false,
                    SortOrder_ = _sortOrder
                };
                await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Circuit_Db_Proxy>().AddUiTemplateAsync(imported);
                Emit(new Template_List_Changed());
            }
        }
    }
}
