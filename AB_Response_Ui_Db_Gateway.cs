using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core;
using ArtificialBuilder_EDP.Core.Messaging;
using EDPFW;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>활성 Response UI DB 에 대한 요청 처리 게이트웨이 ([[app-logic-separation]] — 화면 구성).
    /// 토픽: AB_Response_Ui_Db_Topics.ActiveResponseUi.
    /// Window / Component / UiTemplate 처리 = OLD request types (AB_Get_All_Windows_Request 등) — Topic 만 ActiveResponseUi 로 redirect 한 채 그대로 처리.</summary>
    public class AB_Response_Ui_Db_Gateway : ArtificialBuilder_EDP.Core.AB_Component
    {
        private IAB_Message_Broker? m_broker;
        private AB_Subscription_Token? m_sub;

        public override void OnAttach()
        {
            try
            {
                m_broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
                m_sub = m_broker.Subscribe(AB_Response_Ui_Db_Topics.ActiveResponseUi, HandleMessage);
            }
            catch (Exception ex)
            {
                AB_Log.Error("ResponseUiGw", $"OnAttach 실패: {ex.Message}");
            }
        }

        public override void OnDetach()
        {
            try
            {
                if (m_broker != null && m_sub != null)
                {
                    m_broker.Unsubscribe(m_sub);
                }
            }
            catch { }
        }

        private async void HandleMessage(AB_Message _msg)
        {
            if (_msg.IsResponse) return;

            try
            {
                int dbId = ActiveDbId;

                switch (_msg)
                {
                    // ==================== Meta ====================
                    case AB_Get_Response_Ui_Meta_Request req:
                    {
                        AB_Response_Ui_Meta_Model? data = dbId == 0
                            ? null
                            : await AB_Board.Db.GetByIdAsync<AB_Response_Ui_Meta_Model>(dbId, "meta");
                        m_broker?.Publish(new AB_Get_Response_Ui_Meta_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Save_Response_Ui_Meta_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Update(dbId, req.Meta);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Save_Response_Ui_Meta_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    // ==================== Layers ====================
                    case AB_Get_All_Response_Ui_Layers_Request req:
                    {
                        List<AB_Response_Ui_Layer_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Response_Ui_Layer_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Response_Ui_Layers_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Response_Ui_Layer_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Response_Ui_Layer_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Save_Response_Ui_Layer_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Update(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Save_Response_Ui_Layer_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Delete_Response_Ui_Layer_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Remove(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Delete_Response_Ui_Layer_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    // ==================== Windows (OLD types relocated) ====================
                    case AB_Get_All_Windows_Request req:
                    {
                        List<AB_Response_Ui_Window_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Response_Ui_Window_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Windows_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_Window_Request req:
                    {
                        AB_Response_Ui_Window_Model? data = dbId == 0
                            ? null
                            : await AB_Board.Db.GetByIdAsync<AB_Response_Ui_Window_Model>(dbId, req.Id);
                        m_broker?.Publish(new AB_Get_Window_Response
                        {
                            CorrelationId = req.CorrelationId,
                            Data = data,
                            IsOk = data != null
                        });
                        break;
                    }
                    case AB_Add_Window_Request req:
                    {
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Window);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Add_Window_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Save_Window_Request req:
                    {
                        if (dbId != 0)
                        {
                            req.Window.UpdatedAt_ = DateTime.UtcNow;
                            AB_Board.Db.Update(dbId, req.Window);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Save_Window_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Delete_Window_Request req:
                    {
                        if (dbId != 0)
                        {
                            var window = await AB_Board.Db.GetByIdAsync<AB_Response_Ui_Window_Model>(dbId, req.Id);
                            if (window != null)
                            {
                                AB_Board.Db.Remove(dbId, window);
                                await AB_Board.Db.SaveChangesAsync(dbId);
                            }
                        }
                        m_broker?.Publish(new AB_Delete_Window_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    // ==================== WindowComponents (OLD types relocated) ====================
                    case AB_Get_Window_Components_Request req:
                    {
                        List<AB_Response_Ui_Component_Model> data = new();
                        if (dbId != 0)
                        {
                            string wid = req.WindowId;
                            var found = await AB_Board.Db.FindAsync<AB_Response_Ui_Component_Model>(dbId, _c => _c.WindowId_ == wid);
                            data.AddRange(found);
                        }
                        m_broker?.Publish(new AB_Get_Window_Components_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Get_All_Window_Components_Request req:
                    {
                        List<AB_Response_Ui_Component_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Response_Ui_Component_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Window_Components_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Window_Component_Request req:
                    {
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Component);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Add_Window_Component_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Save_Window_Component_Request req:
                    {
                        if (dbId != 0)
                        {
                            req.Component.UpdatedAt_ = DateTime.UtcNow;
                            AB_Board.Db.Update(dbId, req.Component);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Save_Window_Component_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Delete_Window_Component_Request req:
                    {
                        if (dbId != 0)
                        {
                            var entity = await AB_Board.Db.GetByIdAsync<AB_Response_Ui_Component_Model>(dbId, req.Id);
                            if (entity != null)
                            {
                                AB_Board.Db.Remove(dbId, entity);
                                await AB_Board.Db.SaveChangesAsync(dbId);
                            }
                        }
                        m_broker?.Publish(new AB_Delete_Window_Component_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Delete_Window_Components_By_Window_Request req:
                    {
                        int deleted = 0;
                        if (dbId != 0)
                        {
                            string wid = req.WindowId;
                            var found = await AB_Board.Db.FindAsync<AB_Response_Ui_Component_Model>(dbId, _c => _c.WindowId_ == wid);
                            foreach (var c in found)
                            {
                                AB_Board.Db.Remove(dbId, c);
                                deleted++;
                            }
                            if (deleted > 0)
                            {
                                await AB_Board.Db.SaveChangesAsync(dbId);
                            }
                        }
                        m_broker?.Publish(new AB_Delete_Window_Components_By_Window_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0, DeletedCount = deleted });
                        break;
                    }
                    // ==================== UiTemplates (OLD types relocated) ====================
                    case AB_Get_All_Ui_Templates_Request req:
                    {
                        var data = await UiTemplatesGetAllAsync();
                        m_broker?.Publish(new AB_Get_All_Ui_Templates_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Ui_Template_Request req:
                    {
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Template);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Add_Ui_Template_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Save_Ui_Template_Request req:
                    {
                        if (dbId != 0)
                        {
                            AB_Board.Db.Update(dbId, req.Template);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                        }
                        m_broker?.Publish(new AB_Save_Ui_Template_Response
                        { CorrelationId = req.CorrelationId, Success = dbId != 0 });
                        break;
                    }
                    case AB_Delete_Ui_Template_Request req:
                    {
                        bool ok = await UiTemplateDeleteAsync(req.Template);
                        m_broker?.Publish(new AB_Delete_Ui_Template_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Set_Active_Ui_Template_Request req:
                    {
                        bool ok = await UiTemplateSetActiveAsync(req.TemplateId);
                        m_broker?.Publish(new AB_Set_Active_Ui_Template_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_Active_Ui_Template_Request req:
                    {
                        var data = await UiTemplateGetActiveAsync();
                        m_broker?.Publish(new AB_Get_Active_Ui_Template_Response
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
                AB_Log.Error("ResponseUiGw", $"{_msg.GetType().Name} 처리 실패: {detail}");
                PublishFallbackError(_msg, ex.Message);
            }
        }

        /// <summary>catch 블록 fallback — 매칭되는 Response 타입으로 에러 응답 발행 (timeout 방지).</summary>
        private void PublishFallbackError(AB_Message _req, string _error)
        {
            try
            {
                AB_Message? resp = _req switch
                {
                    AB_Get_All_Windows_Request r => new AB_Get_All_Windows_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_Window_Request r => new AB_Get_Window_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Add_Window_Request r => new AB_Add_Window_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Save_Window_Request r => new AB_Save_Window_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Delete_Window_Request r => new AB_Delete_Window_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Get_Window_Components_Request r => new AB_Get_Window_Components_Response
                    { CorrelationId = r.CorrelationId, Error = _error },
                    AB_Get_All_Window_Components_Request r => new AB_Get_All_Window_Components_Response
                    { CorrelationId = r.CorrelationId, Error = _error },
                    AB_Add_Window_Component_Request r => new AB_Add_Window_Component_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Save_Window_Component_Request r => new AB_Save_Window_Component_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Delete_Window_Component_Request r => new AB_Delete_Window_Component_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Delete_Window_Components_By_Window_Request r => new AB_Delete_Window_Components_By_Window_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Get_All_Ui_Templates_Request r => new AB_Get_All_Ui_Templates_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Get_Active_Ui_Template_Request r => new AB_Get_Active_Ui_Template_Response
                    { CorrelationId = r.CorrelationId },
                    AB_Add_Ui_Template_Request r => new AB_Add_Ui_Template_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Save_Ui_Template_Request r => new AB_Save_Ui_Template_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Delete_Ui_Template_Request r => new AB_Delete_Ui_Template_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Set_Active_Ui_Template_Request r => new AB_Set_Active_Ui_Template_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Save_Response_Ui_Meta_Request r => new AB_Save_Response_Ui_Meta_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Add_Response_Ui_Layer_Request r => new AB_Add_Response_Ui_Layer_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Save_Response_Ui_Layer_Request r => new AB_Save_Response_Ui_Layer_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    AB_Delete_Response_Ui_Layer_Request r => new AB_Delete_Response_Ui_Layer_Response
                    { CorrelationId = r.CorrelationId, Success = false, Error = _error },
                    _ => null
                };
                if (resp != null)
                {
                    m_broker?.Publish(resp);
                }
            }
            catch { }
        }

        // --- UiTemplate 헬퍼 (Engine 직결) ---

        private static async Task<List<AB_Response_Ui_Template_Model>> UiTemplatesGetAllAsync()
        {
            List<AB_Response_Ui_Template_Model> result = new();
            int dbId = ActiveDbId;
            if (dbId == 0) return result;
            var all = await AB_Board.Db.GetAllAsync<AB_Response_Ui_Template_Model>(dbId);
            result.AddRange(all);
            result.Sort(CompareUiTemplateBySortOrder);
            return result;
        }

        private static async Task<bool> UiTemplateDeleteAsync(AB_Response_Ui_Template_Model _template)
        {
            int dbId = ActiveDbId;
            if (dbId == 0) return false;
            var all = await AB_Board.Db.GetAllAsync<AB_Response_Ui_Template_Model>(dbId);
            int count = 0;
            foreach (var _ in all)
            {
                count++;
                if (count > 1) break;
            }
            if (count <= 1) return false;
            AB_Board.Db.Remove(dbId, _template);
            await AB_Board.Db.SaveChangesAsync(dbId);
            return true;
        }

        private static async Task<bool> UiTemplateSetActiveAsync(string _templateId)
        {
            int dbId = ActiveDbId;
            if (dbId == 0) return false;
            var all = await AB_Board.Db.GetAllAsync<AB_Response_Ui_Template_Model>(dbId);
            foreach (AB_Response_Ui_Template_Model t in all)
            {
                t.IsActive_ = (t.Id_ == _templateId);
                AB_Board.Db.Update(dbId, t);
            }
            await AB_Board.Db.SaveChangesAsync(dbId);
            return true;
        }

        private static async Task<AB_Response_Ui_Template_Model?> UiTemplateGetActiveAsync()
        {
            int dbId = ActiveDbId;
            if (dbId == 0) return null;
            var all = await AB_Board.Db.GetAllAsync<AB_Response_Ui_Template_Model>(dbId);
            AB_Response_Ui_Template_Model? active = null;
            AB_Response_Ui_Template_Model? first = null;
            foreach (AB_Response_Ui_Template_Model t in all)
            {
                if (first == null) first = t;
                if (t.IsActive_)
                {
                    active = t;
                    break;
                }
            }
            return active ?? first;
        }

        private static int CompareUiTemplateBySortOrder(AB_Response_Ui_Template_Model _a, AB_Response_Ui_Template_Model _b)
        {
            return _a.SortOrder_.CompareTo(_b.SortOrder_);
        }

        private static int ActiveDbId => AB_Board.ResponseUi.Handle;
    }
}
