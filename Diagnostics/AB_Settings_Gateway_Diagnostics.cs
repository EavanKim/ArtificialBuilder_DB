using ArtificialBuilder;
using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Threading.Tasks;

namespace ArtificialBuilder_EDP.Core.Diagnostics
{
    /// <summary>공통: 진단 동안 임시 Circuit을 열고 정리하는 헬퍼.</summary>
    public static class Temp_Circuit_Scope
    {
        public static async Task<string> OpenAsync()
        {
            var circuit = ArtificialBuilder_EDP.AB_Board.Circuit;
            string name = $"diag_test_{DateTime.UtcNow.Ticks}";
            await circuit.OpenAsync(name);
            return name;
        }

        public static async Task CloseAndDeleteAsync(string _name)
        {
            try
            {
                var circuit = ArtificialBuilder_EDP.AB_Board.Circuit;
                await circuit.CloseAsync();
                string path = $"circuit/{_name}.circuit";
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
            catch { }
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // 11. SaveSettings_RoundTrip
    // ─────────────────────────────────────────────────────────────────
    /// <summary>11. AB_Save_Settings_Request 게이트웨이 round-trip + 저장된 값을 GetSettings로 재조회 검증.</summary>
    public class AB_Test_Gateway_Save_Settings_Round_Trip : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_SaveSettings_RoundTrip";
        /// <inheritdoc/>
        public override string Category => "Gateway/Settings";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();
            Log("circuit.name", circuitName);

            try
            {
                Step("기본 settings 먼저 Add (빈 Circuit이라 row가 없을 수 있음)");
                var initial = new AB_Circuit_Settings_Model
                {
                    HomeMessage_ = "initial home",
                    LlmInitPrompt_ = "initial prompt"
                };
                var addResp = await broker.PublishAndWaitAsync<AB_Add_Settings_Response>(
                    new AB_Add_Settings_Request { Settings = initial }, TimeSpan.FromSeconds(5));
                Log("add.Success", addResp.Success);
                Log("add.Error", addResp.Error ?? "<null>");
                Assert("Add 성공", addResp.Success, addResp.Error ?? "");

                Step("Save로 값 갱신");
                var updated = new AB_Circuit_Settings_Model
                {
                    HomeMessage_ = "updated home",
                    LlmInitPrompt_ = "updated prompt"
                };
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var saveResp = await broker.PublishAndWaitAsync<AB_Save_Settings_Response>(
                    new AB_Save_Settings_Request { Settings = updated }, TimeSpan.FromSeconds(5));
                sw.Stop();
                Log("save.Success", saveResp.Success);
                Log("save.Error", saveResp.Error ?? "<null>");
                Log("save.elapsed", $"{sw.Elapsed.TotalMilliseconds:F1}ms");
                Assert("Save 성공", saveResp.Success, saveResp.Error ?? "");
                Assert("응답 IsResponse=true", saveResp.IsResponse);

                Step("GetSettings로 재조회해 갱신 확인");
                var getResp = await broker.PublishAndWaitAsync<AB_Get_Circuit_Settings_Response>(
                    new AB_Get_Circuit_Settings_Request(), TimeSpan.FromSeconds(5));
                Log("get.IsOk", getResp.IsOk);
                Log("get.HomeMessage_", getResp.Data?.HomeMessage_ ?? "<null>");
                Log("get.LlmInitPrompt_", getResp.Data?.LlmInitPrompt_ ?? "<null>");
                Assert("Get 성공", getResp.IsOk);
                Assert("HomeMessage 갱신됨", getResp.Data?.HomeMessage_ == "updated home",
                    $"got={getResp.Data?.HomeMessage_}");
                Assert("LlmInitPrompt 갱신됨", getResp.Data?.LlmInitPrompt_ == "updated prompt",
                    $"got={getResp.Data?.LlmInitPrompt_}");
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // 12. AddSettings_RoundTrip
    // ─────────────────────────────────────────────────────────────────
    /// <summary>12. AB_Add_Settings_Request 게이트웨이 round-trip 검증 (신규 row 삽입).</summary>
    public class AB_Test_Gateway_Add_Settings_Round_Trip : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_AddSettings_RoundTrip";
        /// <inheritdoc/>
        public override string Category => "Gateway/Settings";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();
            Log("circuit.name", circuitName);

            try
            {
                Step("AddSettings 발행");
                var settings = new AB_Circuit_Settings_Model
                {
                    HomeMessage_ = "added via gateway",
                    LlmInitPrompt_ = "first prompt"
                };
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var addResp = await broker.PublishAndWaitAsync<AB_Add_Settings_Response>(
                    new AB_Add_Settings_Request { Settings = settings }, TimeSpan.FromSeconds(5));
                sw.Stop();
                Log("add.Success", addResp.Success);
                Log("add.Error", addResp.Error ?? "<null>");
                Log("add.elapsed", $"{sw.Elapsed.TotalMilliseconds:F1}ms");
                Assert("Add 성공", addResp.Success, addResp.Error ?? "");
                Assert("응답 IsResponse=true", addResp.IsResponse);

                Step("GetSettings 로 삽입 확인");
                var getResp = await broker.PublishAndWaitAsync<AB_Get_Circuit_Settings_Response>(
                    new AB_Get_Circuit_Settings_Request(), TimeSpan.FromSeconds(5));
                Log("get.IsOk", getResp.IsOk);
                Log("get.HomeMessage_", getResp.Data?.HomeMessage_ ?? "<null>");
                Assert("Get 성공", getResp.IsOk);
                Assert("HomeMessage 일치", getResp.Data?.HomeMessage_ == "added via gateway",
                    $"got={getResp.Data?.HomeMessage_}");
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }
}
