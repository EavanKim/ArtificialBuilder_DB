using System;
using System.Collections.Generic;
using EDPFW;
using ArtificialBuilder_EDP.Core;
using ArtificialBuilder;
using ArtificialBuilder.DDO;
using ArtificialBuilder.Models;

namespace ArtificialBuilder_EDP.Components
{
    /// <summary>AB_Template_Service 래핑 컴포넌트. DB Proxy 만 사용 — 별도 의존 주입 없음.
    /// (windows-ddo-migration sub 3) UI 측 직접 호출 X — DDO 옵저버 등록 + handler 가 본체 Instance 위임.</summary>
    public class AB_Template_Service_Component : ArtificialBuilder_EDP.Core.AB_Component
    {
        /// <summary>래핑된 템플릿 서비스</summary>
        private AB_Template_Service m_instance = new();
        public AB_Template_Service Instance
        {
            get { return m_instance; }
            private set { m_instance = value; }
        }

        public override void OnAttach()
        {
            Instance.Initialize();
            AB_DDO_Subscription_Manager mgr = AB_Engine.GetService<AB_DDO_Subscription_Manager>();
            mgr.AddObserverFor(this, AB_Object_Command_Type.TEMPLATE_GET_ALL_CIRCUIT, HandleGetAllCircuit);
            mgr.AddObserverFor(this, AB_Object_Command_Type.TEMPLATE_ADD_CIRCUIT, HandleAddCircuit);
            mgr.AddObserverFor(this, AB_Object_Command_Type.TEMPLATE_DELETE_CIRCUIT, HandleDeleteCircuit);
            mgr.AddObserverFor(this, AB_Object_Command_Type.TEMPLATE_GET_ALL_GLOBAL, HandleGetAllGlobal);
            mgr.AddObserverFor(this, AB_Object_Command_Type.TEMPLATE_IMPORT_FROM_GLOBAL, HandleImportFromGlobal);
            mgr.AddObserverFor(this, AB_Object_Command_Type.TEMPLATE_SAVE_CIRCUIT, HandleSaveCircuit);
            mgr.AddObserverFor(this, AB_Object_Command_Type.TEMPLATE_SET_ACTIVE_CIRCUIT, HandleSetActiveCircuit);
        }

        public override void OnDetach()
        {
            AB_Engine.GetService<AB_DDO_Subscription_Manager>().UnregisterOwner(this);
        }

        private static void PublishResult(AB_Object_Command_Type _header, AB_Id? _queryId, object _result)
        {
            if (_queryId == null) return;
            if (!AB_Engine.TryGet<AB_DDO_Subscription_Manager>(out var ddo)) return;
            ddo.Publish(_header, null, _result, _toId: _queryId);
        }

        private void HandleGetAllCircuit(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            async void RunAsync()
            {
                List<AB_Response_Ui_Template_Model> ts = await m_instance.GetAllCircuitAsync();
                var result = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Template_Get_All_Circuit_Result>();
                result.Templates_ = ts;
                PublishResult(AB_Object_Command_Type.TEMPLATE_GET_ALL_CIRCUIT, queryId, result);
            }
            RunAsync();
        }

        private void HandleAddCircuit(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            AB_Template_Add_Circuit_Payload? p = _cmd.Payload as AB_Template_Add_Circuit_Payload;
            if (p == null || p.Template_ == null) return;
            AB_Response_Ui_Template_Model t = p.Template_;
            async void RunAsync()
            {
                await m_instance.AddCircuitAsync(t);
                var result = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Template_Add_Circuit_Result>();
                result.Ok_ = true;
                PublishResult(AB_Object_Command_Type.TEMPLATE_ADD_CIRCUIT, queryId, result);
            }
            RunAsync();
        }

        private void HandleDeleteCircuit(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            AB_Template_Delete_Circuit_Payload? p = _cmd.Payload as AB_Template_Delete_Circuit_Payload;
            if (p == null || p.Template_ == null) return;
            AB_Response_Ui_Template_Model t = p.Template_;
            async void RunAsync()
            {
                bool ok = await m_instance.DeleteCircuitAsync(t);
                var result = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Template_Delete_Circuit_Result>();
                result.Ok_ = ok;
                PublishResult(AB_Object_Command_Type.TEMPLATE_DELETE_CIRCUIT, queryId, result);
            }
            RunAsync();
        }

        private void HandleGetAllGlobal(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            async void RunAsync()
            {
                List<AB_Ui_Template_Model> ts = await m_instance.GetAllGlobalAsync();
                var result = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Template_Get_All_Global_Result>();
                result.Templates_ = ts;
                PublishResult(AB_Object_Command_Type.TEMPLATE_GET_ALL_GLOBAL, queryId, result);
            }
            RunAsync();
        }

        private void HandleImportFromGlobal(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            AB_Template_Import_From_Global_Payload? p = _cmd.Payload as AB_Template_Import_From_Global_Payload;
            if (p == null) return;
            string gid = p.GlobalTemplateId_;
            int sortOrder = p.SortOrder_;
            async void RunAsync()
            {
                await m_instance.ImportFromGlobalAsync(gid, sortOrder);
                var result = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Template_Import_From_Global_Result>();
                result.Ok_ = true;
                PublishResult(AB_Object_Command_Type.TEMPLATE_IMPORT_FROM_GLOBAL, queryId, result);
            }
            RunAsync();
        }

        private void HandleSaveCircuit(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            AB_Template_Save_Circuit_Payload? p = _cmd.Payload as AB_Template_Save_Circuit_Payload;
            if (p == null || p.Template_ == null) return;
            AB_Response_Ui_Template_Model t = p.Template_;
            async void RunAsync()
            {
                await m_instance.SaveCircuitAsync(t);
                var result = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Template_Save_Circuit_Result>();
                result.Ok_ = true;
                PublishResult(AB_Object_Command_Type.TEMPLATE_SAVE_CIRCUIT, queryId, result);
            }
            RunAsync();
        }

        private void HandleSetActiveCircuit(AB_DDO_Command _cmd)
        {
            AB_Id? queryId = _cmd.FromId;
            if (queryId == null) return;
            AB_Template_Set_Active_Circuit_Payload? p = _cmd.Payload as AB_Template_Set_Active_Circuit_Payload;
            if (p == null) return;
            string templateId = p.TemplateId_;
            async void RunAsync()
            {
                await m_instance.SetActiveCircuitAsync(templateId);
                var result = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Template_Set_Active_Circuit_Result>();
                result.Ok_ = true;
                PublishResult(AB_Object_Command_Type.TEMPLATE_SET_ACTIVE_CIRCUIT, queryId, result);
            }
            RunAsync();
        }
    }
}
