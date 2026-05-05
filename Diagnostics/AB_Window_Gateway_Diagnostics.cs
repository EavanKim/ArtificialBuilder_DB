using ArtificialBuilder;
using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Threading.Tasks;

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
                var initial = await broker.PublishAndWaitAsync<AB_Get_All_Windows_Response>(
                    new AB_Get_All_Windows_Request(), TimeSpan.FromSeconds(5));
                int initialCount = initial.Data.Count;
                Log("initial.Count", initialCount);

                Step("Add window");
                var w = new AB_Response_Ui_Window_Model { Name_ = "test_w" };
                var addResp = await broker.PublishAndWaitAsync<AB_Add_Window_Response>(
                    new AB_Add_Window_Request { Window = w }, TimeSpan.FromSeconds(5));
                Assert("Add 성공", addResp.Success, addResp.Error ?? "");

                Step("Save (rename)");
                w.Name_ = "test_w_renamed";
                var saveResp = await broker.PublishAndWaitAsync<AB_Save_Window_Response>(
                    new AB_Save_Window_Request { Window = w }, TimeSpan.FromSeconds(5));
                Assert("Save 성공", saveResp.Success, saveResp.Error ?? "");

                Step("GetWindow 단건");
                var getResp = await broker.PublishAndWaitAsync<AB_Get_Window_Response>(
                    new AB_Get_Window_Request { Id = w.Id_ }, TimeSpan.FromSeconds(5));
                Log("get.IsOk", getResp.IsOk);
                Assert("조회됨", getResp.IsOk && getResp.Data != null);

                Step("Delete");
                var delResp = await broker.PublishAndWaitAsync<AB_Delete_Window_Response>(
                    new AB_Delete_Window_Request { Id = w.Id_ }, TimeSpan.FromSeconds(5));
                Assert("Delete 성공", delResp.Success, delResp.Error ?? "");

                Step("최종 GetAll");
                var final = await broker.PublishAndWaitAsync<AB_Get_All_Windows_Response>(
                    new AB_Get_All_Windows_Request(), TimeSpan.FromSeconds(5));
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
