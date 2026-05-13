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
    /// <summary>19. Relation/Location/Connections — 빈 Circuit에서 GetAll 안전성 검증.</summary>
    public class AB_Test_Gateway_Relation_Location_Get_All_Empty : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_RelationLocation_GetAllEmpty";
        /// <inheritdoc/>
        public override string Category => "Gateway/RelationLocation";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                Step("GetAllRelationships (빈 Circuit)");
                var req1 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Relationships_Request>();
                var rels = await broker.PublishAndWaitAsync<AB_Get_All_Relationships_Response>(req1, TimeSpan.FromSeconds(5));
                Log("relationships.Count", rels.Data.Count);
                Assert("관계 0개", rels.Data.Count == 0);

                Step("GetAllLocations");
                var req2 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Locations_Request>();
                var locs = await broker.PublishAndWaitAsync<AB_Get_All_Locations_Response>(req2, TimeSpan.FromSeconds(5));
                Log("locations.Count", locs.Data.Count);
                Assert("장소 0개", locs.Data.Count == 0);

                Step("GetAllLocationConnections");
                var req3 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Location_Connections_Request>();
                var conns = await broker.PublishAndWaitAsync<AB_Get_All_Location_Connections_Response>(req3, TimeSpan.FromSeconds(5));
                Log("connections.Count", conns.Data.Count);
                Assert("연결 0개", conns.Data.Count == 0);
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }

    /// <summary>20. RelationColor CRUD round-trip.</summary>
    public class AB_Test_Gateway_Relation_Color_Crud : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_RelationColor_Crud";
        /// <inheritdoc/>
        public override string Category => "Gateway/RelationLocation";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                Step("GetAllRelationColors (빈)");
                var req4 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Relation_Colors_Request>();
                var initial = await broker.PublishAndWaitAsync<AB_Get_All_Relation_Colors_Response>(req4, TimeSpan.FromSeconds(5));
                Log("initial.Count", initial.Data.Count);
                Assert("초기 0개", initial.Data.Count == 0);

                Step("Add RelationColor");
                var color = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Relation_Color_Model>();
                color.RelationType_ = "friend";
                var req5 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Add_Relation_Color_Request>();
                req5.Color = color;
                var addResp = await broker.PublishAndWaitAsync<AB_Add_Relation_Color_Response>(req5, TimeSpan.FromSeconds(5));
                Assert("Add 성공", addResp.Success, addResp.Error ?? "");

                Step("Save 직후 (Add tracked 동일 인스턴스)");
                color.RelationType_ = "best_friend";
                var req6 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Save_Relation_Color_Request>();
                req6.Color = color;
                var saveResp = await broker.PublishAndWaitAsync<AB_Save_Relation_Color_Response>(req6, TimeSpan.FromSeconds(5));
                Assert("Save 성공", saveResp.Success, saveResp.Error ?? "");

                Step("Delete 직후");
                var req7 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Delete_Relation_Color_Request>();
                req7.Color = color;
                var delResp = await broker.PublishAndWaitAsync<AB_Delete_Relation_Color_Response>(req7, TimeSpan.FromSeconds(5));
                Assert("Delete 성공", delResp.Success, delResp.Error ?? "");

                Step("GetAll 0개 확인");
                var req8 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Relation_Colors_Request>();
                var final = await broker.PublishAndWaitAsync<AB_Get_All_Relation_Colors_Response>(req8, TimeSpan.FromSeconds(5));
                Log("final.Count", final.Data.Count);
                Assert("최종 0개", final.Data.Count == 0);
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }
}
