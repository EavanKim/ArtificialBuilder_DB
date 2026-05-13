using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 전역 App DB 프록시. 브로커 + AB_App_Db_Gateway 경유. UI/서비스 계층의 단일 진입점.
    /// </summary>
    public class AB_App_Db_Proxy : ArtificialBuilder_EDP.Core.AB_Object
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
        // ModelConfig
        // ============================================================

        /// <summary>모델 설정 전체.</summary>
        public async Task<List<AB_Model_Config_Model>> GetAllModelsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Models_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Models_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>ID로 모델 단건. (ddo-datakey-typed sub 2) string → long 시그니처 마이그.</summary>
        public async Task<AB_Db_Result<AB_Model_Config_Model>> GetModelByIdAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Model_By_Id_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Model_By_Id_Response>(req, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Model_Config_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Model_Config_Model>.NotFound();
        }

        /// <summary>모델 추가.</summary>
        public async Task AddModelAsync(AB_Model_Config_Model _model)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Model_Request>();
            req.Model = _model;
            await GetBroker().PublishAndWaitAsync<AB_Add_Model_Response>(req, DefaultTimeout);
        }

        /// <summary>모델 갱신.</summary>
        public async Task UpdateModelAsync(AB_Model_Config_Model _model)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Update_Model_Request>();
            req.Model = _model;
            await GetBroker().PublishAndWaitAsync<AB_Update_Model_Response>(req, DefaultTimeout);
        }

        /// <summary>모델 삭제.</summary>
        public async Task DeleteModelAsync(AB_Model_Config_Model _model)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Model_Request>();
            req.Model = _model;
            await GetBroker().PublishAndWaitAsync<AB_Delete_Model_Response>(req, DefaultTimeout);
        }

        // ============================================================
        // UiTemplate (글로벌)
        // ============================================================

        /// <summary>전역 UI 템플릿 전체.</summary>
        public async Task<List<AB_Ui_Template_Model>> GetAllUiTemplatesAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_App_Ui_Templates_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_App_Ui_Templates_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>ID로 글로벌 UI 템플릿.</summary>
        public async Task<AB_Db_Result<AB_Ui_Template_Model>> GetUiTemplateByIdAsync(string _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_App_Ui_Template_By_Id_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_App_Ui_Template_By_Id_Response>(req, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Ui_Template_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Ui_Template_Model>.NotFound();
        }

        /// <summary>글로벌 UI 템플릿 추가.</summary>
        public async Task AddUiTemplateAsync(AB_Ui_Template_Model _template)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_App_Ui_Template_Request>();
            req.Template = _template;
            await GetBroker().PublishAndWaitAsync<AB_Add_App_Ui_Template_Response>(req, DefaultTimeout);
        }

        /// <summary>글로벌 UI 템플릿 갱신.</summary>
        public async Task UpdateUiTemplateAsync(AB_Ui_Template_Model _template)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Update_App_Ui_Template_Request>();
            req.Template = _template;
            await GetBroker().PublishAndWaitAsync<AB_Update_App_Ui_Template_Response>(req, DefaultTimeout);
        }

        /// <summary>글로벌 UI 템플릿 삭제.</summary>
        public async Task DeleteUiTemplateAsync(AB_Ui_Template_Model _template)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_App_Ui_Template_Request>();
            req.Template = _template;
            await GetBroker().PublishAndWaitAsync<AB_Delete_App_Ui_Template_Response>(req, DefaultTimeout);
        }

        // ============================================================
        // Pipeline (레거시)
        // ============================================================

        /// <summary>파이프라인 전체.</summary>
        public async Task<List<AB_Pipeline_Model>> GetAllPipelinesAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Pipelines_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Pipelines_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>ID로 파이프라인 단건.</summary>
        public async Task<AB_Db_Result<AB_Pipeline_Model>> GetPipelineByIdAsync(string _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Pipeline_By_Id_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Pipeline_By_Id_Response>(req, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Pipeline_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Pipeline_Model>.NotFound();
        }

        /// <summary>파이프라인 추가.</summary>
        public async Task AddPipelineAsync(AB_Pipeline_Model _pipeline)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Pipeline_Request>();
            req.Pipeline = _pipeline;
            await GetBroker().PublishAndWaitAsync<AB_Add_Pipeline_Response>(req, DefaultTimeout);
        }

        /// <summary>파이프라인 갱신.</summary>
        public async Task UpdatePipelineAsync(AB_Pipeline_Model _pipeline)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Update_Pipeline_Request>();
            req.Pipeline = _pipeline;
            await GetBroker().PublishAndWaitAsync<AB_Update_Pipeline_Response>(req, DefaultTimeout);
        }

        /// <summary>파이프라인 삭제.</summary>
        public async Task DeletePipelineAsync(AB_Pipeline_Model _pipeline)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Pipeline_Request>();
            req.Pipeline = _pipeline;
            await GetBroker().PublishAndWaitAsync<AB_Delete_Pipeline_Response>(req, DefaultTimeout);
        }

        // ============================================================
        // Llama_Model (typed-id-edp-rebase chunk 4o)
        // ============================================================

        /// <summary>Llama 모델 전체 조회.</summary>
        public async Task<List<AB_Llama_Model>> GetAllLlamaModelsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Llama_Models_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_Llama_Models_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>Id 로 Llama 모델 단건.</summary>
        public async Task<AB_Db_Result<AB_Llama_Model>> GetLlamaModelByIdAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Llama_Model_By_Id_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Llama_Model_By_Id_Response>(req, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Llama_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Llama_Model>.NotFound();
        }

        /// <summary>FileName 으로 Llama 모델 단건 조회 (scan-on-load upsert 용).</summary>
        public async Task<AB_Db_Result<AB_Llama_Model>> GetLlamaModelByFileNameAsync(string _fileName)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Llama_Model_By_File_Name_Request>();
            req.FileName = _fileName;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_Llama_Model_By_File_Name_Response>(req, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_Llama_Model>.Ok(resp.Data);
            return AB_Db_Result<AB_Llama_Model>.NotFound();
        }

        /// <summary>Llama 모델 upsert (filename unique). 반환 = 저장된 row (Id 포함).</summary>
        public async Task<AB_Llama_Model?> UpsertLlamaModelAsync(AB_Llama_Model _model)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Upsert_Llama_Model_Request>();
            req.Model = _model;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Upsert_Llama_Model_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        /// <summary>Llama 모델 삭제 (Id 기반).</summary>
        public async Task<bool> DeleteLlamaModelAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Llama_Model_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_Llama_Model_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // ============================================================
        // HF_Download (typed-id-edp-rebase chunk 4p)
        // ============================================================

        /// <summary>HF 다운로드 전체 조회.</summary>
        public async Task<List<AB_HF_Download>> GetAllHfDownloadsAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_HF_Downloads_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_HF_Downloads_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>Id 로 HF 다운로드 단건.</summary>
        public async Task<AB_Db_Result<AB_HF_Download>> GetHfDownloadByIdAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_HF_Download_By_Id_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_HF_Download_By_Id_Response>(req, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_HF_Download>.Ok(resp.Data);
            return AB_Db_Result<AB_HF_Download>.NotFound();
        }

        /// <summary>HF 다운로드 추가 (Enqueue 시 — long Id 발급). 반환 = 저장된 row.</summary>
        public async Task<AB_HF_Download?> AddHfDownloadAsync(AB_HF_Download _model)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_HF_Download_Request>();
            req.Model = _model;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Add_HF_Download_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        /// <summary>HF 다운로드 삭제 (Id 기반).</summary>
        public async Task<bool> DeleteHfDownloadAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_HF_Download_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_HF_Download_Response>(req, DefaultTimeout);
            return resp.Success;
        }

        // ============================================================
        // HF_Repo (typed-id-edp-rebase chunk 4q)
        // ============================================================

        /// <summary>HF repo 전체 조회.</summary>
        public async Task<List<AB_HF_Repo>> GetAllHfReposAsync()
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_HF_Repos_Request>();
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_All_HF_Repos_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        /// <summary>Id 로 HF repo 단건.</summary>
        public async Task<AB_Db_Result<AB_HF_Repo>> GetHfRepoByIdAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_HF_Repo_By_Id_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_HF_Repo_By_Id_Response>(req, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_HF_Repo>.Ok(resp.Data);
            return AB_Db_Result<AB_HF_Repo>.NotFound();
        }

        /// <summary>RepoId ("owner/repo") 로 HF repo 단건.</summary>
        public async Task<AB_Db_Result<AB_HF_Repo>> GetHfRepoByRepoIdAsync(string _repoId)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_HF_Repo_By_Repo_Id_Request>();
            req.RepoId = _repoId;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Get_HF_Repo_By_Repo_Id_Response>(req, DefaultTimeout);
            if (resp.IsOk && resp.Data != null) return AB_Db_Result<AB_HF_Repo>.Ok(resp.Data);
            return AB_Db_Result<AB_HF_Repo>.NotFound();
        }

        /// <summary>HF repo upsert (repo_id unique). 반환 = 저장된 row.</summary>
        public async Task<AB_HF_Repo?> UpsertHfRepoAsync(AB_HF_Repo _model)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Upsert_HF_Repo_Request>();
            req.Model = _model;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Upsert_HF_Repo_Response>(req, DefaultTimeout);
            return resp.Data;
        }

        /// <summary>HF repo 삭제 (Id 기반).</summary>
        public async Task<bool> DeleteHfRepoAsync(long _id)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_HF_Repo_Request>();
            req.Id = _id;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Delete_HF_Repo_Response>(req, DefaultTimeout);
            return resp.Success;
        }
    }
}
