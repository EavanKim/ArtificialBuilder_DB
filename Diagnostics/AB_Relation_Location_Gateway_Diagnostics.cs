using ArtificialBuilder;
using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Threading.Tasks;

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
                var rels = await broker.PublishAndWaitAsync<AB_Get_All_Relationships_Response>(
                    new AB_Get_All_Relationships_Request(), TimeSpan.FromSeconds(5));
                Log("relationships.Count", rels.Data.Count);
                Assert("관계 0개", rels.Data.Count == 0);

                Step("GetAllLocations");
                var locs = await broker.PublishAndWaitAsync<AB_Get_All_Locations_Response>(
                    new AB_Get_All_Locations_Request(), TimeSpan.FromSeconds(5));
                Log("locations.Count", locs.Data.Count);
                Assert("장소 0개", locs.Data.Count == 0);

                Step("GetAllLocationConnections");
                var conns = await broker.PublishAndWaitAsync<AB_Get_All_Location_Connections_Response>(
                    new AB_Get_All_Location_Connections_Request(), TimeSpan.FromSeconds(5));
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
                var initial = await broker.PublishAndWaitAsync<AB_Get_All_Relation_Colors_Response>(
                    new AB_Get_All_Relation_Colors_Request(), TimeSpan.FromSeconds(5));
                Log("initial.Count", initial.Data.Count);
                Assert("초기 0개", initial.Data.Count == 0);

                Step("Add RelationColor");
                var color = new AB_Relation_Color_Model { RelationType_ = "friend" };
                var addResp = await broker.PublishAndWaitAsync<AB_Add_Relation_Color_Response>(
                    new AB_Add_Relation_Color_Request { Color = color }, TimeSpan.FromSeconds(5));
                Assert("Add 성공", addResp.Success, addResp.Error ?? "");

                Step("Save 직후 (Add tracked 동일 인스턴스)");
                color.RelationType_ = "best_friend";
                var saveResp = await broker.PublishAndWaitAsync<AB_Save_Relation_Color_Response>(
                    new AB_Save_Relation_Color_Request { Color = color }, TimeSpan.FromSeconds(5));
                Assert("Save 성공", saveResp.Success, saveResp.Error ?? "");

                Step("Delete 직후");
                var delResp = await broker.PublishAndWaitAsync<AB_Delete_Relation_Color_Response>(
                    new AB_Delete_Relation_Color_Request { Color = color }, TimeSpan.FromSeconds(5));
                Assert("Delete 성공", delResp.Success, delResp.Error ?? "");

                Step("GetAll 0개 확인");
                var final = await broker.PublishAndWaitAsync<AB_Get_All_Relation_Colors_Response>(
                    new AB_Get_All_Relation_Colors_Request(), TimeSpan.FromSeconds(5));
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
