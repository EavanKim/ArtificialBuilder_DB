using ArtificialBuilder;
using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Threading.Tasks;
using ArtificialBuilder_EDP.Core;

namespace ArtificialBuilder_EDP.Core.Diagnostics
{
    /// <summary>22. UiTemplate — Add/Save/SetActive/GetActive round-trip.</summary>
    public class AB_Test_Gateway_Ui_Template_Crud : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_UiTemplate_Crud";
        /// <inheritdoc/>
        public override string Category => "Gateway/UiTemplate";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                Step("초기 GetAll");
                var req1 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Ui_Templates_Request>();
                var initial = await broker.PublishAndWaitAsync<AB_Get_All_Ui_Templates_Response>(req1, TimeSpan.FromSeconds(5));
                int initialCount = initial.Data.Count;
                Log("initial.Count", initialCount);

                Step("Add 2 templates");
                var t1 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Response_Ui_Template_Model>();
                t1.Name_ = "tpl_a";
                t1.SortOrder_ = 100;
                var t2 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Response_Ui_Template_Model>();
                t2.Name_ = "tpl_b";
                t2.SortOrder_ = 101;
                var req2 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Ui_Template_Request>();
                req2.Template = t1;
                var addResp1 = await broker.PublishAndWaitAsync<AB_Add_Ui_Template_Response>(req2, TimeSpan.FromSeconds(5));
                Assert("Add t1 성공", addResp1.Success, addResp1.Error ?? "");
                var req3 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Ui_Template_Request>();
                req3.Template = t2;
                var addResp2 = await broker.PublishAndWaitAsync<AB_Add_Ui_Template_Response>(req3, TimeSpan.FromSeconds(5));
                Assert("Add t2 성공", addResp2.Success, addResp2.Error ?? "");

                Step("Save (rename t1)");
                t1.Name_ = "tpl_a_renamed";
                var req4 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Ui_Template_Request>();
                req4.Template = t1;
                var saveResp = await broker.PublishAndWaitAsync<AB_Save_Ui_Template_Response>(req4, TimeSpan.FromSeconds(5));
                Assert("Save 성공", saveResp.Success, saveResp.Error ?? "");

                Step("SetActive(t2)");
                var req5 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Set_Active_Ui_Template_Request>();
                req5.TemplateId = t2.Id_.ToString();
                var setResp = await broker.PublishAndWaitAsync<AB_Set_Active_Ui_Template_Response>(req5, TimeSpan.FromSeconds(5));
                Assert("SetActive 성공", setResp.Success, setResp.Error ?? "");

                Step("GetActive == t2");
                var req6 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Active_Ui_Template_Request>();
                var actResp = await broker.PublishAndWaitAsync<AB_Get_Active_Ui_Template_Response>(req6, TimeSpan.FromSeconds(5));
                Log("active.Id", actResp.Data?.Id_.ToString() ?? "(null)");
                Assert("활성 t2", actResp.Data != null && actResp.Data.Id_ == t2.Id_);

                Step("Delete t1");
                var req7 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Ui_Template_Request>();
                req7.Template = t1;
                var delResp = await broker.PublishAndWaitAsync<AB_Delete_Ui_Template_Response>(req7, TimeSpan.FromSeconds(5));
                Assert("Delete 성공", delResp.Success, delResp.Error ?? "");

                Step("최종 GetAll");
                var req8 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Ui_Templates_Request>();
                var final = await broker.PublishAndWaitAsync<AB_Get_All_Ui_Templates_Response>(req8, TimeSpan.FromSeconds(5));
                Log("final.Count", final.Data.Count);
                bool t1Deleted = true;
                bool t2Exists = false;
                foreach (var item in final.Data)
                {
                    if (item.Id_ == t1.Id_) t1Deleted = false;
                    if (item.Id_ == t2.Id_) t2Exists = true;
                }
                Assert("t1 삭제됨", t1Deleted);
                Assert("t2 잔존", t2Exists);
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }
}
