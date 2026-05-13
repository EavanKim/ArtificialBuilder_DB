using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using EDPFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 전역 App DB 게이트웨이. 브로커 토픽 db.app 구독, EDP_Db_Engine(AB_Board.Db) 직결 호출.
    /// AB_Board.App.Handle 0 인 경우(미초기화) 빈/false 응답.
    /// </summary>
    public class AB_App_Db_Gateway : ArtificialBuilder_EDP.Core.AB_Component
    {
        private IAB_Message_Broker? m_broker;
        private AB_Subscription_Token? m_sub;
        private EDP_Db_Engine m_engine => AB_Board.Db;

        /// <inheritdoc/>
        public override void OnAttach()
        {
            try
            {
                m_broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
                m_sub = m_broker.Subscribe(AB_App_Db_Topics.App, HandleMessage);
            }
            catch (Exception ex)
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Error("AppGw", $"OnAttach 실패: {ex.Message}");
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

        private static int Handle => AB_Board.App?.Handle ?? 0;

        private async void HandleMessage(AB_Message _msg)
        {
            if (_msg.IsResponse) return;

            try
            {
                int handle = Handle;

                switch (_msg)
                {
                    // ============================================================
                    // ModelConfig
                    // ============================================================
                    case AB_Get_All_Models_Request req:
                    {
                        List<AB_Model_Config_Model> data = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.GetAllAsync<AB_Model_Config_Model>(handle);
                            data = all.ToList();
                        }
                        m_broker?.Publish(new AB_Get_All_Models_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_Model_By_Id_Request req:
                    {
                        AB_Model_Config_Model? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_Model_Config_Model>(handle, _m => _m.Id_ == req.Id);
                            data = found.FirstOrDefault();
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_Model_By_Id_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Add_Model_Request req:
                    {
                        if (handle != 0)
                        {
                            await m_engine.AddAsync(handle, req.Model);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Add_Model_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }
                    case AB_Update_Model_Request req:
                    {
                        if (handle != 0)
                        {
                            m_engine.Update(handle, req.Model);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Update_Model_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }
                    case AB_Delete_Model_Request req:
                    {
                        if (handle != 0)
                        {
                            m_engine.Remove(handle, req.Model);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Delete_Model_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }

                    // ============================================================
                    // UiTemplate (글로벌)
                    // ============================================================
                    case AB_Get_All_App_Ui_Templates_Request req:
                    {
                        List<AB_Ui_Template_Model> data = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.GetAllAsync<AB_Ui_Template_Model>(handle);
                            data = all.ToList();
                        }
                        m_broker?.Publish(new AB_Get_All_App_Ui_Templates_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_App_Ui_Template_By_Id_Request req:
                    {
                        AB_Ui_Template_Model? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            data = await m_engine.GetByIdAsync<AB_Ui_Template_Model>(handle, req.Id);
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_App_Ui_Template_By_Id_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Add_App_Ui_Template_Request req:
                    {
                        if (handle != 0)
                        {
                            await m_engine.AddAsync(handle, req.Template);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Add_App_Ui_Template_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }
                    case AB_Update_App_Ui_Template_Request req:
                    {
                        if (handle != 0)
                        {
                            m_engine.Update(handle, req.Template);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Update_App_Ui_Template_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }
                    case AB_Delete_App_Ui_Template_Request req:
                    {
                        if (handle != 0)
                        {
                            m_engine.Remove(handle, req.Template);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Delete_App_Ui_Template_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }

                    // ============================================================
                    // Pipeline (레거시)
                    // ============================================================
                    case AB_Get_All_Pipelines_Request req:
                    {
                        List<AB_Pipeline_Model> data = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.GetAllAsync<AB_Pipeline_Model>(handle);
                            data = all.ToList();
                        }
                        m_broker?.Publish(new AB_Get_All_Pipelines_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_Pipeline_By_Id_Request req:
                    {
                        AB_Pipeline_Model? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            data = await m_engine.GetByIdAsync<AB_Pipeline_Model>(handle, req.Id);
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_Pipeline_By_Id_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Add_Pipeline_Request req:
                    {
                        if (handle != 0)
                        {
                            await m_engine.AddAsync(handle, req.Pipeline);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Add_Pipeline_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }
                    case AB_Update_Pipeline_Request req:
                    {
                        if (handle != 0)
                        {
                            m_engine.Update(handle, req.Pipeline);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Update_Pipeline_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }
                    case AB_Delete_Pipeline_Request req:
                    {
                        if (handle != 0)
                        {
                            m_engine.Remove(handle, req.Pipeline);
                            await m_engine.SaveChangesAsync(handle);
                        }
                        m_broker?.Publish(new AB_Delete_Pipeline_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0 });
                        break;
                    }

                    // ============================================================
                    // Llama_Model (typed-id-edp-rebase chunk 4o)
                    // ============================================================
                    case AB_Get_All_Llama_Models_Request req:
                    {
                        List<AB_Llama_Model> data = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.GetAllAsync<AB_Llama_Model>(handle);
                            data = all.ToList();
                        }
                        m_broker?.Publish(new AB_Get_All_Llama_Models_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_Llama_Model_By_Id_Request req:
                    {
                        AB_Llama_Model? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_Llama_Model>(handle, _m => _m.Id_ == req.Id);
                            data = found.FirstOrDefault();
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_Llama_Model_By_Id_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Get_Llama_Model_By_File_Name_Request req:
                    {
                        AB_Llama_Model? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_Llama_Model>(handle, _m => _m.FileName_ == req.FileName);
                            data = found.FirstOrDefault();
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_Llama_Model_By_File_Name_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Upsert_Llama_Model_Request req:
                    {
                        AB_Llama_Model? saved = null;
                        bool success = false;
                        if (handle != 0)
                        {
                            // filename unique key — 존재 시 update, 없으면 add
                            var existingList = await m_engine.FindAsync<AB_Llama_Model>(handle, _m => _m.FileName_ == req.Model.FileName_);
                            AB_Llama_Model? existing = existingList.FirstOrDefault();
                            if (existing != null)
                            {
                                existing.FilePath_ = req.Model.FilePath_;
                                existing.FileSizeBytes_ = req.Model.FileSizeBytes_;
                                existing.UpdatedAt_ = DateTime.UtcNow;
                                m_engine.Update(handle, existing);
                                saved = existing;
                            }
                            else
                            {
                                req.Model.CreatedAt_ = DateTime.UtcNow;
                                req.Model.UpdatedAt_ = DateTime.UtcNow;
                                await m_engine.AddAsync(handle, req.Model);
                                saved = req.Model;
                            }
                            await m_engine.SaveChangesAsync(handle);
                            success = true;
                        }
                        m_broker?.Publish(new AB_Upsert_Llama_Model_Response
                        { CorrelationId = req.CorrelationId, Data = saved, Success = success });
                        break;
                    }
                    case AB_Delete_Llama_Model_Request req:
                    {
                        bool success = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_Llama_Model>(handle, _m => _m.Id_ == req.Id);
                            AB_Llama_Model? row = found.FirstOrDefault();
                            if (row != null)
                            {
                                m_engine.Remove(handle, row);
                                await m_engine.SaveChangesAsync(handle);
                                success = true;
                            }
                        }
                        m_broker?.Publish(new AB_Delete_Llama_Model_Response
                        { CorrelationId = req.CorrelationId, Success = success });
                        break;
                    }

                    // ============================================================
                    // HF_Download (typed-id-edp-rebase chunk 4p)
                    // ============================================================
                    case AB_Get_All_HF_Downloads_Request req:
                    {
                        List<AB_HF_Download> data = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.GetAllAsync<AB_HF_Download>(handle);
                            data = all.ToList();
                        }
                        m_broker?.Publish(new AB_Get_All_HF_Downloads_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_HF_Download_By_Id_Request req:
                    {
                        AB_HF_Download? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_HF_Download>(handle, _m => _m.Id_ == req.Id);
                            data = found.FirstOrDefault();
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_HF_Download_By_Id_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Add_HF_Download_Request req:
                    {
                        AB_HF_Download? saved = null;
                        bool success = false;
                        if (handle != 0)
                        {
                            req.Model.CreatedAt_ = DateTime.UtcNow;
                            req.Model.UpdatedAt_ = DateTime.UtcNow;
                            await m_engine.AddAsync(handle, req.Model);
                            await m_engine.SaveChangesAsync(handle);
                            saved = req.Model;
                            success = true;
                        }
                        m_broker?.Publish(new AB_Add_HF_Download_Response
                        { CorrelationId = req.CorrelationId, Data = saved, Success = success });
                        break;
                    }
                    case AB_Delete_HF_Download_Request req:
                    {
                        bool success = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_HF_Download>(handle, _m => _m.Id_ == req.Id);
                            AB_HF_Download? row = found.FirstOrDefault();
                            if (row != null)
                            {
                                m_engine.Remove(handle, row);
                                await m_engine.SaveChangesAsync(handle);
                                success = true;
                            }
                        }
                        m_broker?.Publish(new AB_Delete_HF_Download_Response
                        { CorrelationId = req.CorrelationId, Success = success });
                        break;
                    }

                    // ============================================================
                    // HF_Repo (typed-id-edp-rebase chunk 4q)
                    // ============================================================
                    case AB_Get_All_HF_Repos_Request req:
                    {
                        List<AB_HF_Repo> data = new();
                        if (handle != 0)
                        {
                            var all = await m_engine.GetAllAsync<AB_HF_Repo>(handle);
                            data = all.ToList();
                        }
                        m_broker?.Publish(new AB_Get_All_HF_Repos_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_HF_Repo_By_Id_Request req:
                    {
                        AB_HF_Repo? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_HF_Repo>(handle, _m => _m.Id_ == req.Id);
                            data = found.FirstOrDefault();
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_HF_Repo_By_Id_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Get_HF_Repo_By_Repo_Id_Request req:
                    {
                        AB_HF_Repo? data = null;
                        bool isOk = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_HF_Repo>(handle, _m => _m.RepoId_ == req.RepoId);
                            data = found.FirstOrDefault();
                            isOk = data != null;
                        }
                        m_broker?.Publish(new AB_Get_HF_Repo_By_Repo_Id_Response
                        { CorrelationId = req.CorrelationId, Data = data, IsOk = isOk });
                        break;
                    }
                    case AB_Upsert_HF_Repo_Request req:
                    {
                        AB_HF_Repo? saved = null;
                        bool success = false;
                        if (handle != 0)
                        {
                            var existingList = await m_engine.FindAsync<AB_HF_Repo>(handle, _m => _m.RepoId_ == req.Model.RepoId_);
                            AB_HF_Repo? existing = existingList.FirstOrDefault();
                            if (existing != null)
                            {
                                existing.Description_ = req.Model.Description_;
                                existing.License_ = req.Model.License_;
                                existing.UpdatedAt_ = DateTime.UtcNow;
                                m_engine.Update(handle, existing);
                                saved = existing;
                            }
                            else
                            {
                                req.Model.CreatedAt_ = DateTime.UtcNow;
                                req.Model.UpdatedAt_ = DateTime.UtcNow;
                                await m_engine.AddAsync(handle, req.Model);
                                saved = req.Model;
                            }
                            await m_engine.SaveChangesAsync(handle);
                            success = true;
                        }
                        m_broker?.Publish(new AB_Upsert_HF_Repo_Response
                        { CorrelationId = req.CorrelationId, Data = saved, Success = success });
                        break;
                    }
                    case AB_Delete_HF_Repo_Request req:
                    {
                        bool success = false;
                        if (handle != 0)
                        {
                            var found = await m_engine.FindAsync<AB_HF_Repo>(handle, _m => _m.Id_ == req.Id);
                            AB_HF_Repo? row = found.FirstOrDefault();
                            if (row != null)
                            {
                                m_engine.Remove(handle, row);
                                await m_engine.SaveChangesAsync(handle);
                                success = true;
                            }
                        }
                        m_broker?.Publish(new AB_Delete_HF_Repo_Response
                        { CorrelationId = req.CorrelationId, Success = success });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Error("AppGw", $"HandleMessage 실패: {ex.Message}");
            }
        }
    }
}
