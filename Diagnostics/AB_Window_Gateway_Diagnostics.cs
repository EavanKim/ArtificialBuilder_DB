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
    /// <summary>25. Window — Add/Save/Delete round-trip.</summary>
    public class AB_Test_Gateway_Window_Crud : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_Window_Crud";
        /// <inheritdoc/>
        public override string Category => "Gateway/Window";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                Step("초기 GetAllWindows");
                var req1 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Windows_Request>();
                var initial = await broker.PublishAndWaitAsync<AB_Get_All_Windows_Response>(req1, TimeSpan.FromSeconds(5));
                int initialCount = initial.Data.Count;
                Log("initial.Count", initialCount);

                Step("Add window");
                var w = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Response_Ui_Window_Model>();
                w.Name_ = "test_w";
                var req2 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Window_Request>();
                req2.Window = w;
                var addResp = await broker.PublishAndWaitAsync<AB_Add_Window_Response>(req2, TimeSpan.FromSeconds(5));
                Assert("Add 성공", addResp.Success, addResp.Error ?? "");

                Step("Save (rename)");
                w.Name_ = "test_w_renamed";
                var req3 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Window_Request>();
                req3.Window = w;
                var saveResp = await broker.PublishAndWaitAsync<AB_Save_Window_Response>(req3, TimeSpan.FromSeconds(5));
                Assert("Save 성공", saveResp.Success, saveResp.Error ?? "");

                Step("GetWindow 단건");
                var req4 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Window_Request>();
                req4.Id = w.Id_;
                var getResp = await broker.PublishAndWaitAsync<AB_Get_Window_Response>(req4, TimeSpan.FromSeconds(5));
                Log("get.IsOk", getResp.IsOk);
                Assert("조회됨", getResp.IsOk && getResp.Data != null);

                Step("Delete");
                var req5 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Window_Request>();
                req5.Id = w.Id_;
                var delResp = await broker.PublishAndWaitAsync<AB_Delete_Window_Response>(req5, TimeSpan.FromSeconds(5));
                Assert("Delete 성공", delResp.Success, delResp.Error ?? "");

                Step("최종 GetAll");
                var req6 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Windows_Request>();
                var final = await broker.PublishAndWaitAsync<AB_Get_All_Windows_Response>(req6, TimeSpan.FromSeconds(5));
                Log("final.Count", final.Data.Count);
                bool allDeleted = true;
                foreach (var item in final.Data)
                {
                    if (item.Id_ == w.Id_) { allDeleted = false; break; }
                }
                Assert("삭제됨", allDeleted);
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }
}
