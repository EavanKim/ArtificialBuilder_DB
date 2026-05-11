using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Threading.Tasks;

namespace ArtificialBuilder_EDP.Core.Diagnostics
{
    /// <summary>26. Vec — 미초기화 상태 read/write 안전성 round-trip.</summary>
    /// <remarks>
    /// 임시 Circuit은 vec 저장소가 초기화되지 않은 상태로 열린다 (호출처에서 InitializeVec 미호출).
    /// 이 상태에서 read 계열은 빈/0 을 반환해야 하고, write 계열은 silent no-op 으로 성공해야 한다.
    /// 게이트웨이 분기 + write 계열 catch fallback 회귀 방지를 목적으로 한다.
    /// </remarks>
    public class AB_Test_Gateway_Vec_Round_Trip : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_Vec_RoundTrip";
        /// <inheritdoc/>
        public override string Category => "Gateway/Vec";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();
            var to = TimeSpan.FromSeconds(5);

            try
            {
                Step("IsVecInitialized");
                var initResp = await broker.PublishAndWaitAsync<AB_Is_Vec_Initialized_Response>(
                    new AB_Is_Vec_Initialized_Request(), to);
                Log("initialized", initResp.Initialized);
                Assert("미초기화 상태", initResp.Initialized == false);

                Step("GetVecTotalRowCount");
                var cntResp = await broker.PublishAndWaitAsync<AB_Get_Vec_Total_Row_Count_Response>(
                    new AB_Get_Vec_Total_Row_Count_Request(), to);
                Log("rowCount", cntResp.Count);
                Assert("0행", cntResp.Count == 0);

                Step("GetChatEmbeddingsBySession (빈)");
                var listResp = await broker.PublishAndWaitAsync<AB_Get_Chat_Embeddings_By_Session_Response>(
                    new AB_Get_Chat_Embeddings_By_Session_Request { SessionId = 1L }, to);
                Assert("빈 리스트", listResp.Data.Count == 0);

                Step("SearchLore (빈)");
                var sl = await broker.PublishAndWaitAsync<AB_Search_Lore_Vec_Response>(
                    new AB_Search_Lore_Vec_Request { Query = new float[] { 0.1f, 0.2f }, TopK = 5 }, to);
                Assert("Lore hits 0", sl.Hits.Count == 0);

                Step("SearchChat (빈)");
                var sc = await broker.PublishAndWaitAsync<AB_Search_Chat_Vec_Response>(
                    new AB_Search_Chat_Vec_Request { Query = new float[] { 0.1f, 0.2f }, TopK = 5 }, to);
                Assert("Chat hits 0", sc.Hits.Count == 0);

                Step("SearchCData (빈)");
                var scd = await broker.PublishAndWaitAsync<AB_Search_CData_Vec_Response>(
                    new AB_Search_CData_Vec_Request { Query = new float[] { 0.1f, 0.2f }, TopK = 5 }, to);
                Assert("CData hits 0", scd.Hits.Count == 0);

                Step("Insert/Delete LoreEmbedding (silent no-op)");
                var ile = await broker.PublishAndWaitAsync<AB_Insert_Lore_Embedding_Response>(
                    new AB_Insert_Lore_Embedding_Request { LoreId = "lore_x", Embedding = new float[] { 0.1f } }, to);
                Assert("InsertLoreEmbedding 성공", ile.Success, ile.Error ?? "");
                var dle = await broker.PublishAndWaitAsync<AB_Delete_Lore_Embedding_Response>(
                    new AB_Delete_Lore_Embedding_Request { LoreId = "lore_x" }, to);
                Assert("DeleteLoreEmbedding 성공", dle.Success, dle.Error ?? "");

                Step("Insert/Delete ChatEmbedding (silent no-op)");
                var ice = await broker.PublishAndWaitAsync<AB_Insert_Chat_Embedding_Response>(
                    new AB_Insert_Chat_Embedding_Request
                    {
                        SessionId = 1L,
                        NodeId = 1L,
                        TurnIndex = 1,
                        RefreshIndex = 0,
                        EmissionOrder = 0,
                        Embedding = new float[] { 0.1f }
                    }, to);
                Assert("InsertChatEmbedding 성공", ice.Success, ice.Error ?? "");
                var dcebr = await broker.PublishAndWaitAsync<AB_Delete_Chat_Embedding_By_Record_Response>(
                    new AB_Delete_Chat_Embedding_By_Record_Request
                    {
                        SessionId = 1L,
                        NodeId = 1L,
                        TurnIndex = 1,
                        RefreshIndex = 0,
                        EmissionOrder = 0
                    }, to);
                Assert("DeleteByRecord 성공", dcebr.Success, dcebr.Error ?? "");
                var dcebs = await broker.PublishAndWaitAsync<AB_Delete_Chat_Embeddings_By_Session_Response>(
                    new AB_Delete_Chat_Embeddings_By_Session_Request { SessionId = 1L }, to);
                Assert("DeleteBySession 성공", dcebs.Success, dcebs.Error ?? "");

                Step("InsertCDataEmbedding (silent no-op)");
                var icde = await broker.PublishAndWaitAsync<AB_Insert_CData_Embedding_Response>(
                    new AB_Insert_CData_Embedding_Request { CDataId = "cd_x", Embedding = new float[] { 0.1f } }, to);
                Assert("InsertCDataEmbedding 성공", icde.Success, icde.Error ?? "");

                Step("ClearAllVec (silent no-op)");
                var cav = await broker.PublishAndWaitAsync<AB_Clear_All_Vec_Response>(
                    new AB_Clear_All_Vec_Request(), to);
                Assert("ClearAllVec 성공", cav.Success, cav.Error ?? "");

                Step("OpenVecFile");
                var ovf = await broker.PublishAndWaitAsync<AB_Open_Vec_File_Response>(
                    new AB_Open_Vec_File_Request(), to);
                Assert("OpenVecFile 성공", ovf.Success, ovf.Error ?? "");

                Step("RenameVecFile (없는 파일 → false)");
                var rvf = await broker.PublishAndWaitAsync<AB_Rename_Vec_File_Response>(
                    new AB_Rename_Vec_File_Request { OldName = "__none__", NewName = "__none2__" }, to);
                Log("rename(미존재).Success", rvf.Success);
                // 미존재 파일은 false 가 정상 — 게이트웨이 분기 자체가 동작했음을 확인
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }
}
