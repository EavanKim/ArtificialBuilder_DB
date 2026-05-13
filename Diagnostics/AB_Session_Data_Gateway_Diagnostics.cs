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
                var req1 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Session_Data_Request>();
                req1.SessionId = 1L;
                var sd = await broker.PublishAndWaitAsync<AB_Get_All_Session_Data_Response>(req1, TimeSpan.FromSeconds(5));
                Log("sessionData.Count", sd.Data.Count);
                Assert("0개", sd.Data.Count == 0);

                Step("GetAllDataCategories (빈)");
                var req2 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Data_Categories_Request>();
                var cats = await broker.PublishAndWaitAsync<AB_Get_All_Data_Categories_Response>(req2, TimeSpan.FromSeconds(5));
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
                var cat = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Data_Category_Model>();
                cat.Name_ = "test_cat";
                catId = cat.Id_.ToString();
                var req3 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Data_Category_Request>();
                req3.Category = cat;
                var addCatResp = await broker.PublishAndWaitAsync<AB_Add_Data_Category_Response>(req3, TimeSpan.FromSeconds(5));
                Assert("AddCategory 성공", addCatResp.Success, addCatResp.Error ?? "");

                Step("UpsertCharacterData");
                var req4 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Upsert_Character_Data_Request>();
                req4.CharacterId = charId;
                req4.SessionId = long.Parse(sessionId);
                req4.CategoryId = catId;
                req4.FieldName = "mood";
                req4.FieldValue = "happy";
                req4.Source = "test";
                req4.MessageId = 1;
                var upResp = await broker.PublishAndWaitAsync<AB_Upsert_Character_Data_Response>(req4, TimeSpan.FromSeconds(5));
                Assert("Upsert 성공", upResp.Success, upResp.Error ?? "");

                Step("GetCharacterDataByCategory");
                var req5 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Character_Data_By_Category_Request>();
                req5.CharacterId = charId;
                req5.SessionId = long.Parse(sessionId);
                req5.CategoryId = catId;
                var byCat = await broker.PublishAndWaitAsync<AB_Get_Character_Data_By_Category_Response>(req5, TimeSpan.FromSeconds(5));
                Log("byCategory.Count", byCat.Data.Count);
                Assert("1건 이상", byCat.Data.Count >= 1);

                Step("GetAllSessionData");
                var req6 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Session_Data_Request>();
                req6.SessionId = long.Parse(sessionId);
                var sd = await broker.PublishAndWaitAsync<AB_Get_All_Session_Data_Response>(req6, TimeSpan.FromSeconds(5));
                Log("sessionData.Count", sd.Data.Count);
                Assert("세션 데이터 1건 이상", sd.Data.Count >= 1);

                Step("DeleteSessionData");
                var req7 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Session_Data_Request>();
                req7.SessionId = long.Parse(sessionId);
                var delResp = await broker.PublishAndWaitAsync<AB_Delete_Session_Data_Response>(req7, TimeSpan.FromSeconds(5));
                Assert("Delete 성공", delResp.Success, delResp.Error ?? "");

                Step("최종 GetAllSessionData == 0");
                var req8 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Session_Data_Request>();
                req8.SessionId = long.Parse(sessionId);
                var final = await broker.PublishAndWaitAsync<AB_Get_All_Session_Data_Response>(req8, TimeSpan.FromSeconds(5));
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
