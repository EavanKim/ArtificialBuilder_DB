using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Threading.Tasks;
using ArtificialBuilder_EDP.Core;

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
                var req1 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Is_Vec_Initialized_Request>();
                var initResp = await broker.PublishAndWaitAsync<AB_Is_Vec_Initialized_Response>(req1, to);
                Log("initialized", initResp.Initialized);
                Assert("미초기화 상태", initResp.Initialized == false);

                Step("GetVecTotalRowCount");
                var req2 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Vec_Total_Row_Count_Request>();
                var cntResp = await broker.PublishAndWaitAsync<AB_Get_Vec_Total_Row_Count_Response>(req2, to);
                Log("rowCount", cntResp.Count);
                Assert("0행", cntResp.Count == 0);

                Step("GetChatEmbeddingsBySession (빈)");
                var req3 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_Chat_Embeddings_By_Session_Request>();
                req3.SessionId = 1L;
                var listResp = await broker.PublishAndWaitAsync<AB_Get_Chat_Embeddings_By_Session_Response>(req3, to);
                Assert("빈 리스트", listResp.Data.Count == 0);

                Step("SearchLore (빈)");
                var reqSL = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Search_Lore_Vec_Request>();
                reqSL.Query = new float[] { 0.1f, 0.2f };
                reqSL.TopK = 5;
                var sl = await broker.PublishAndWaitAsync<AB_Search_Lore_Vec_Response>(reqSL, to);
                Assert("Lore hits 0", sl.Hits.Count == 0);

                Step("SearchChat (빈)");
                var reqSC = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Search_Chat_Vec_Request>();
                reqSC.Query = new float[] { 0.1f, 0.2f };
                reqSC.TopK = 5;
                var sc = await broker.PublishAndWaitAsync<AB_Search_Chat_Vec_Response>(reqSC, to);
                Assert("Chat hits 0", sc.Hits.Count == 0);

                Step("SearchCData (빈)");
                var reqSCD = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Search_CData_Vec_Request>();
                reqSCD.Query = new float[] { 0.1f, 0.2f };
                reqSCD.TopK = 5;
                var scd = await broker.PublishAndWaitAsync<AB_Search_CData_Vec_Response>(reqSCD, to);
                Assert("CData hits 0", scd.Hits.Count == 0);

                Step("Insert/Delete LoreEmbedding (silent no-op)");
                var reqILE = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Insert_Lore_Embedding_Request>();
                reqILE.LoreId = "lore_x";
                reqILE.Embedding = new float[] { 0.1f };
                var ile = await broker.PublishAndWaitAsync<AB_Insert_Lore_Embedding_Response>(reqILE, to);
                Assert("InsertLoreEmbedding 성공", ile.Success, ile.Error ?? "");
                var req4 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Lore_Embedding_Request>();
                req4.LoreId = "lore_x";
                var dle = await broker.PublishAndWaitAsync<AB_Delete_Lore_Embedding_Response>(req4, to);
                Assert("DeleteLoreEmbedding 성공", dle.Success, dle.Error ?? "");

                Step("Insert/Delete ChatEmbedding (silent no-op)");
                var reqICE = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Insert_Chat_Embedding_Request>();
                reqICE.SessionId = 1L;
                reqICE.NodeId = 1L;
                reqICE.TurnIndex = 1;
                reqICE.RefreshIndex = 0;
                reqICE.EmissionOrder = 0;
                reqICE.Embedding = new float[] { 0.1f };
                var ice = await broker.PublishAndWaitAsync<AB_Insert_Chat_Embedding_Response>(reqICE, to);
                Assert("InsertChatEmbedding 성공", ice.Success, ice.Error ?? "");
                var req5 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Chat_Embedding_By_Record_Request>();
                req5.SessionId = 1L;
                req5.NodeId = 1L;
                req5.TurnIndex = 1;
                req5.RefreshIndex = 0;
                req5.EmissionOrder = 0;
                var dcebr = await broker.PublishAndWaitAsync<AB_Delete_Chat_Embedding_By_Record_Response>(req5, to);
                Assert("DeleteByRecord 성공", dcebr.Success, dcebr.Error ?? "");
                var req6 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Chat_Embeddings_By_Session_Request>();
                req6.SessionId = 1L;
                var dcebs = await broker.PublishAndWaitAsync<AB_Delete_Chat_Embeddings_By_Session_Response>(req6, to);
                Assert("DeleteBySession 성공", dcebs.Success, dcebs.Error ?? "");

                Step("InsertCDataEmbedding (silent no-op)");
                var reqICDE = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Insert_CData_Embedding_Request>();
                reqICDE.CDataId = "cd_x";
                reqICDE.Embedding = new float[] { 0.1f };
                var icde = await broker.PublishAndWaitAsync<AB_Insert_CData_Embedding_Response>(reqICDE, to);
                Assert("InsertCDataEmbedding 성공", icde.Success, icde.Error ?? "");

                Step("ClearAllVec (silent no-op)");
                var req7 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Clear_All_Vec_Request>();
                var cav = await broker.PublishAndWaitAsync<AB_Clear_All_Vec_Response>(req7, to);
                Assert("ClearAllVec 성공", cav.Success, cav.Error ?? "");

                Step("OpenVecFile");
                var req8 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Open_Vec_File_Request>();
                var ovf = await broker.PublishAndWaitAsync<AB_Open_Vec_File_Response>(req8, to);
                Assert("OpenVecFile 성공", ovf.Success, ovf.Error ?? "");

                Step("RenameVecFile (없는 파일 → false)");
                var req9 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Rename_Vec_File_Request>();
                req9.OldName = "__none__";
                req9.NewName = "__none2__";
                var rvf = await broker.PublishAndWaitAsync<AB_Rename_Vec_File_Response>(req9, to);
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
