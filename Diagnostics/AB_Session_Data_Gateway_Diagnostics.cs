using ArtificialBuilder;
using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;

using System.Threading.Tasks;

namespace ArtificialBuilder_EDP.Core.Diagnostics
{
    /// <summary>23. SessionData — 빈 GetAll 안전성.</summary>
    public class AB_Test_Gateway_Session_Data_Get_All_Empty : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_SessionData_GetAllEmpty";
        /// <inheritdoc/>
        public override string Category => "Gateway/SessionData";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                Step("GetAllSessionData (빈)");
                var sd = await broker.PublishAndWaitAsync<AB_Get_All_Session_Data_Response>(
                    new AB_Get_All_Session_Data_Request { SessionId = 1L }, TimeSpan.FromSeconds(5));
                Log("sessionData.Count", sd.Data.Count);
                Assert("0개", sd.Data.Count == 0);

                Step("GetAllDataCategories (빈)");
                var cats = await broker.PublishAndWaitAsync<AB_Get_All_Data_Categories_Response>(
                    new AB_Get_All_Data_Categories_Request(), TimeSpan.FromSeconds(5));
                Log("categories.Count", cats.Data.Count);
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }

    /// <summary>24. SessionData — Upsert/조회/삭제 round-trip.</summary>
    public class AB_Test_Gateway_Session_Data_Crud : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_SessionData_Crud";
        /// <inheritdoc/>
        public override string Category => "Gateway/SessionData";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                string sessionId = "1";
                string charId = "char_x";
                string catId = "cat_x";

                Step("AddDataCategory");
                var cat = new AB_Data_Category_Model { Name_ = "test_cat" };
                catId = cat.Id_.ToString();
                var addCatResp = await broker.PublishAndWaitAsync<AB_Add_Data_Category_Response>(
                    new AB_Add_Data_Category_Request { Category = cat }, TimeSpan.FromSeconds(5));
                Assert("AddCategory 성공", addCatResp.Success, addCatResp.Error ?? "");

                Step("UpsertCharacterData");
                var upResp = await broker.PublishAndWaitAsync<AB_Upsert_Character_Data_Response>(
                    new AB_Upsert_Character_Data_Request
                    {
                        CharacterId = charId, SessionId = long.Parse(sessionId), CategoryId = catId,
                        FieldName = "mood", FieldValue = "happy", Source = "test", MessageId = 1
                    }, TimeSpan.FromSeconds(5));
                Assert("Upsert 성공", upResp.Success, upResp.Error ?? "");

                Step("GetCharacterDataByCategory");
                var byCat = await broker.PublishAndWaitAsync<AB_Get_Character_Data_By_Category_Response>(
                    new AB_Get_Character_Data_By_Category_Request
                    { CharacterId = charId, SessionId = long.Parse(sessionId), CategoryId = catId },
                    TimeSpan.FromSeconds(5));
                Log("byCategory.Count", byCat.Data.Count);
                Assert("1건 이상", byCat.Data.Count >= 1);

                Step("GetAllSessionData");
                var sd = await broker.PublishAndWaitAsync<AB_Get_All_Session_Data_Response>(
                    new AB_Get_All_Session_Data_Request { SessionId = long.Parse(sessionId) }, TimeSpan.FromSeconds(5));
                Log("sessionData.Count", sd.Data.Count);
                Assert("세션 데이터 1건 이상", sd.Data.Count >= 1);

                Step("DeleteSessionData");
                var delResp = await broker.PublishAndWaitAsync<AB_Delete_Session_Data_Response>(
                    new AB_Delete_Session_Data_Request { SessionId = long.Parse(sessionId) }, TimeSpan.FromSeconds(5));
                Assert("Delete 성공", delResp.Success, delResp.Error ?? "");

                Step("최종 GetAllSessionData == 0");
                var final = await broker.PublishAndWaitAsync<AB_Get_All_Session_Data_Response>(
                    new AB_Get_All_Session_Data_Request { SessionId = long.Parse(sessionId) }, TimeSpan.FromSeconds(5));
                Log("final.Count", final.Data.Count);
                Assert("0개", final.Data.Count == 0);
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }
}
