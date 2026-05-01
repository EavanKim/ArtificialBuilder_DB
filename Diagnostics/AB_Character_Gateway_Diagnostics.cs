using ArtificialBuilder;
using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Threading.Tasks;

namespace ArtificialBuilder_EDP.Core.Diagnostics
{
    /// <summary>17. Character Add → Get → Save → GetAll round-trip.</summary>
    public class AB_Test_Gateway_Character_Add_Get_Save : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_Character_AddGetSave";
        /// <inheritdoc/>
        public override string Category => "Gateway/Character";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                Step("Add Character");
                var ch = new AB_Character_Model
                {
                    Name_ = "test_hero",
                    Personality_ = "용감함",
                    Backstory_ = "마을 출신"
                };
                Log("character.Id_", ch.Id_);
                var addResp = await broker.PublishAndWaitAsync<AB_Add_Character_Response>(
                    new AB_Add_Character_Request { Character = ch }, TimeSpan.FromSeconds(5));
                Assert("Add 성공", addResp.Success, addResp.Error ?? "");

                Step("Save 직후 갱신 (Add 시 tracked 동일 인스턴스 — Get 호출은 별도 진단으로 분리)");
                ch.Backstory_ = "왕국의 수도";
                var saveResp = await broker.PublishAndWaitAsync<AB_Save_Character_Response>(
                    new AB_Save_Character_Request { Character = ch }, TimeSpan.FromSeconds(5));
                Assert("Save 성공", saveResp.Success, saveResp.Error ?? "");

                Step("GetAllCharacters 로 갱신 확인");
                var allResp = await broker.PublishAndWaitAsync<AB_Get_All_Characters_Response>(
                    new AB_Get_All_Characters_Request(), TimeSpan.FromSeconds(5));
                Log("all.Count", allResp.Data.Count);
                Log("all[0].Name_", allResp.Data.Count > 0 ? allResp.Data[0].Name_ : "<empty>");
                Log("all[0].Backstory_", allResp.Data.Count > 0 ? (allResp.Data[0].Backstory_ ?? "<null>") : "<empty>");
                Assert("Character 1개", allResp.Data.Count == 1);
                Assert("이름 일치", allResp.Data.Count > 0 && allResp.Data[0].Name_ == "test_hero");
                Assert("Backstory 갱신됨",
                    allResp.Data.Count > 0 && allResp.Data[0].Backstory_ == "왕국의 수도",
                    $"got={(allResp.Data.Count > 0 ? allResp.Data[0].Backstory_ : "<empty>")}");
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }

    /// <summary>18b. GetCharacter 단일 조회 — 별도 fresh Circuit.</summary>
    public class AB_Test_Gateway_Character_Get_Single : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_Character_GetSingle";
        /// <inheritdoc/>
        public override string Category => "Gateway/Character";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                Step("Add Character");
                var ch = new AB_Character_Model { Name_ = "named_one", Personality_ = "용감함" };
                var add = await broker.PublishAndWaitAsync<AB_Add_Character_Response>(
                    new AB_Add_Character_Request { Character = ch }, TimeSpan.FromSeconds(5));
                Assert("Add 성공", add.Success, add.Error ?? "");

                Step("GetCharacter (Id)");
                var get = await broker.PublishAndWaitAsync<AB_Get_Character_Response>(
                    new AB_Get_Character_Request { Id = ch.Id_ }, TimeSpan.FromSeconds(5));
                Log("get.Name_", get.Data?.Name_ ?? "<null>");
                Log("get.Personality_", get.Data?.Personality_ ?? "<null>");
                Assert("결과 존재", get.Data != null);
                Assert("이름 일치", get.Data?.Name_ == "named_one");
                Assert("Personality 일치", get.Data?.Personality_ == "용감함");
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }

    /// <summary>18. Character Add 직후 Delete (EF tracking 충돌 회피 패턴).</summary>
    public class AB_Test_Gateway_Character_Delete : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_Character_Delete";
        /// <inheritdoc/>
        public override string Category => "Gateway/Character";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                Step("Add Character");
                var ch = new AB_Character_Model { Name_ = "to_delete" };
                var addResp = await broker.PublishAndWaitAsync<AB_Add_Character_Response>(
                    new AB_Add_Character_Request { Character = ch }, TimeSpan.FromSeconds(5));
                Assert("Add 성공", addResp.Success, addResp.Error ?? "");

                Step("Delete 직후 (Add 시 tracked 동일 인스턴스)");
                var delResp = await broker.PublishAndWaitAsync<AB_Delete_Character_Response>(
                    new AB_Delete_Character_Request { Character = ch }, TimeSpan.FromSeconds(5));
                Assert("Delete 성공", delResp.Success, delResp.Error ?? "");

                Step("GetAll 로 0개 확인");
                var all = await broker.PublishAndWaitAsync<AB_Get_All_Characters_Response>(
                    new AB_Get_All_Characters_Request(), TimeSpan.FromSeconds(5));
                Log("all.Count", all.Data.Count);
                Assert("Character 0개", all.Data.Count == 0);
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }
}
