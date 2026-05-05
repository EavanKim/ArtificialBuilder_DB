using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// TODO[db-three-way-split]: sub 4 — AB_Db_Manager 흡수. App DB = 글로벌 메타. **로직과 무관** (사용자 정본 [[app-logic-separation]]). plan: docs/plans/doing/db-three-way-split/4-db-manager.md
// TODO[db-three-way-split]: sub 5 — APP_DB_* DDO 헤더 옵저버. plan: docs/plans/doing/db-three-way-split/5-message-queue.md
namespace ArtificialBuilder
{
    /// <summary>
    /// 전역 App DB 프록시. 브로커 + AB_App_Db_Gateway 경유. UI/서비스 계층의 단일 진입점.
    /// </summary>
    public class AB_App_Db_Proxy
    {
        private static AB_App_Db_Proxy? g_instance;
        /// <summary>전역 단일 인스턴스.</summary>
        public static AB_App_Db_Proxy I => g_instance ??= new AB_App_Db_Proxy();

        private TimeSpan m_defaultTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan DefaultTimeout
        {
            get { return m_defaultTimeout; }
            set { m_defaultTimeout = value; }
        }

        private IAB_Message_Broker GetBroker()
            => ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();

        // ============================================================
        // ModelConfig
        // ============================================================

        /// <summary>모델 설정 전체.</summary>
        public async Task<List<AB_Model_Config_Model>> GetAllModelsAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Models_Response>(
                new AB_Get_All_Models_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>ID로 모델 단건.</summary>
        public async Task<AB_Db_Result<AB_Model_Config_Model>> GetModelByIdAsync(string _id)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Model_By_Id_Response>(
                new AB_Get_Model_By_Id_Request { Id = _id }, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Model_Config_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Model_Config_Model>.NotFound();
        }

        /// <summary>모델 추가.</summary>
        public async Task AddModelAsync(AB_Model_Config_Model _model)
        {
            await GetBroker().PublishAndWaitAsync<AB_Add_Model_Response>(
                new AB_Add_Model_Request { Model = _model }, DefaultTimeout);
        }

        /// <summary>모델 갱신.</summary>
        public async Task UpdateModelAsync(AB_Model_Config_Model _model)
        {
            await GetBroker().PublishAndWaitAsync<AB_Update_Model_Response>(
                new AB_Update_Model_Request { Model = _model }, DefaultTimeout);
        }

        /// <summary>모델 삭제.</summary>
        public async Task DeleteModelAsync(AB_Model_Config_Model _model)
        {
            await GetBroker().PublishAndWaitAsync<AB_Delete_Model_Response>(
                new AB_Delete_Model_Request { Model = _model }, DefaultTimeout);
        }

        // ============================================================
        // UiTemplate (글로벌)
        // ============================================================

        /// <summary>전역 UI 템플릿 전체.</summary>
        public async Task<List<AB_Ui_Template_Model>> GetAllUiTemplatesAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_App_Ui_Templates_Response>(
                new AB_Get_All_App_Ui_Templates_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>ID로 글로벌 UI 템플릿.</summary>
        public async Task<AB_Db_Result<AB_Ui_Template_Model>> GetUiTemplateByIdAsync(string _id)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_App_Ui_Template_By_Id_Response>(
                new AB_Get_App_Ui_Template_By_Id_Request { Id = _id }, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Ui_Template_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Ui_Template_Model>.NotFound();
        }

        /// <summary>글로벌 UI 템플릿 추가.</summary>
        public async Task AddUiTemplateAsync(AB_Ui_Template_Model _template)
        {
            await GetBroker().PublishAndWaitAsync<AB_Add_App_Ui_Template_Response>(
                new AB_Add_App_Ui_Template_Request { Template = _template }, DefaultTimeout);
        }

        /// <summary>글로벌 UI 템플릿 갱신.</summary>
        public async Task UpdateUiTemplateAsync(AB_Ui_Template_Model _template)
        {
            await GetBroker().PublishAndWaitAsync<AB_Update_App_Ui_Template_Response>(
                new AB_Update_App_Ui_Template_Request { Template = _template }, DefaultTimeout);
        }

        /// <summary>글로벌 UI 템플릿 삭제.</summary>
        public async Task DeleteUiTemplateAsync(AB_Ui_Template_Model _template)
        {
            await GetBroker().PublishAndWaitAsync<AB_Delete_App_Ui_Template_Response>(
                new AB_Delete_App_Ui_Template_Request { Template = _template }, DefaultTimeout);
        }

        // ============================================================
        // Pipeline (레거시)
        // ============================================================

        /// <summary>파이프라인 전체.</summary>
        public async Task<List<AB_Pipeline_Model>> GetAllPipelinesAsync()
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Pipelines_Response>(
                new AB_Get_All_Pipelines_Request(), DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>ID로 파이프라인 단건.</summary>
        public async Task<AB_Db_Result<AB_Pipeline_Model>> GetPipelineByIdAsync(string _id)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Pipeline_By_Id_Response>(
                new AB_Get_Pipeline_By_Id_Request { Id = _id }, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Pipeline_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Pipeline_Model>.NotFound();
        }

        /// <summary>파이프라인 추가.</summary>
        public async Task AddPipelineAsync(AB_Pipeline_Model _pipeline)
        {
            await GetBroker().PublishAndWaitAsync<AB_Add_Pipeline_Response>(
                new AB_Add_Pipeline_Request { Pipeline = _pipeline }, DefaultTimeout);
        }

        /// <summary>파이프라인 갱신.</summary>
        public async Task UpdatePipelineAsync(AB_Pipeline_Model _pipeline)
        {
            await GetBroker().PublishAndWaitAsync<AB_Update_Pipeline_Response>(
                new AB_Update_Pipeline_Request { Pipeline = _pipeline }, DefaultTimeout);
        }

        /// <summary>파이프라인 삭제.</summary>
        public async Task DeletePipelineAsync(AB_Pipeline_Model _pipeline)
        {
            await GetBroker().PublishAndWaitAsync<AB_Delete_Pipeline_Response>(
                new AB_Delete_Pipeline_Request { Pipeline = _pipeline }, DefaultTimeout);
        }
    }
}
