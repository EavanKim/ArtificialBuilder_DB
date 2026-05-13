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
    /// <summary>13. Lore CRUD round-trip — Add → Get(단일) → Save → GetAll → Delete → GetAll.</summary>
    public class AB_Test_Gateway_Lore_Crud_Round_Trip : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_Lore_CrudRoundTrip";
        /// <inheritdoc/>
        public override string Category => "Gateway/Lore";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();
            Log("circuit.name", circuitName);

            try
            {
                Step("Add Lore Entry");
                var entry = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Lore_Entry_Model>();
                entry.Name_ = "test_lore";
                entry.Keywords_ = "alpha,beta";
                entry.Content_ = "초기 내용";
                Log("entry.Id_", entry.Id_);
                var req1 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Lore_Entry_Request>();
                req1.Entry = entry;
                var addResp = await broker.PublishAndWaitAsync<AB_Add_Lore_Entry_Response>(req1, TimeSpan.FromSeconds(5));
                Log("add.Success", addResp.Success);
                Assert("Add 성공", addResp.Success, addResp.Error ?? "");

                Step("GetLoreEntry 단일 조회");
                var req2 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Lore_Entry_Request>();
                req2.Id = entry.Id_.ToString();
                var getResp = await broker.PublishAndWaitAsync<AB_Get_Lore_Entry_Response>(req2, TimeSpan.FromSeconds(5));
                Log("get.Data.Name_", getResp.Data?.Name_ ?? "<null>");
                Assert("Get 결과 != null", getResp.Data != null);
                Assert("이름 일치", getResp.Data?.Name_ == "test_lore");

                Step("Save로 갱신");
                if (getResp.Data != null)
                {
                    getResp.Data.Content_ = "갱신된 내용";
                    var req3 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Lore_Entry_Request>();
                    req3.Entry = getResp.Data;
                    var saveResp = await broker.PublishAndWaitAsync<AB_Save_Lore_Entry_Response>(req3, TimeSpan.FromSeconds(5));
                    Log("save.Success", saveResp.Success);
                    Assert("Save 성공", saveResp.Success, saveResp.Error ?? "");
                }

                Step("GetAll로 갱신 확인");
                var req4 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Lore_Entries_Request>();
                var allResp = await broker.PublishAndWaitAsync<AB_Get_All_Lore_Entries_Response>(req4, TimeSpan.FromSeconds(5));
                Log("all.Count", allResp.Data.Count);
                Assert("Lore 1개 존재", allResp.Data.Count == 1);
                Assert("내용 갱신됨", allResp.Data.Count > 0 && allResp.Data[0].Content_ == "갱신된 내용",
                    $"got={(allResp.Data.Count > 0 ? allResp.Data[0].Content_ : "<empty>")}");

                Step("Delete");
                if (allResp.Data.Count > 0)
                {
                    var req5 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Lore_Entry_Request>();
                    req5.Entry = allResp.Data[0];
                    var delResp = await broker.PublishAndWaitAsync<AB_Delete_Lore_Entry_Response>(req5, TimeSpan.FromSeconds(5));
                    Log("del.Success", delResp.Success);
                    Assert("Delete 성공", delResp.Success, delResp.Error ?? "");
                }

                Step("GetAll로 삭제 확인");
                var req6 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Lore_Entries_Request>();
                var allResp2 = await broker.PublishAndWaitAsync<AB_Get_All_Lore_Entries_Response>(req6, TimeSpan.FromSeconds(5));
                Log("all.Count (after delete)", allResp2.Data.Count);
                Assert("Lore 0개", allResp2.Data.Count == 0);
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }

    /// <summary>14. FindMatchingLore — keywords 매칭 검색.</summary>
    public class AB_Test_Gateway_Lore_Find_Matching : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_Lore_FindMatching";
        /// <inheritdoc/>
        public override string Category => "Gateway/Lore";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                Step("3개 lore 추가 (다른 키워드)");
                for (int i = 0; i < 3; i++)
                {
                    var entry = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Lore_Entry_Model>();
                    entry.Name_ = $"lore_{i}";
                    entry.Keywords_ = $"key{i}";
                    entry.Content_ = $"content {i}";
                    var req7 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Lore_Entry_Request>();
                    req7.Entry = entry;
                    var resp = await broker.PublishAndWaitAsync<AB_Add_Lore_Entry_Response>(req7, TimeSpan.FromSeconds(5));
                    Assert($"Add {i} 성공", resp.Success);
                }

                Step("FindMatching: 'key1 어쩌고' 텍스트로 검색");
                var req8 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Find_Matching_Lore_Request>();
                req8.Text = "key1 들어간 문장";
                var findResp = await broker.PublishAndWaitAsync<AB_Find_Matching_Lore_Response>(req8, TimeSpan.FromSeconds(5));
                Log("find.Count", findResp.Data.Count);
                foreach (var l in findResp.Data) Log("  match", l.Name_);
                Assert("적어도 1개 매칭", findResp.Data.Count >= 1, $"got {findResp.Data.Count}");
                bool hasLore1 = false;
                foreach (var l in findResp.Data)
                {
                    if (l.Name_ == "lore_1") { hasLore1 = true; break; }
                }
                Assert("lore_1 포함", hasLore1);
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }
}
