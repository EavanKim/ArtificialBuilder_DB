using ArtificialBuilder;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Threading.Tasks;
using ArtificialBuilder_EDP.Core;

namespace ArtificialBuilder_EDP.Core.Diagnostics
{
    /// <summary>21. Pattern — 빈 Circuit에서 GetAll 안전성.</summary>
    public class AB_Test_Gateway_Pattern_Get_All_Empty : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_Pattern_GetAllEmpty";
        /// <inheritdoc/>
        public override string Category => "Gateway/Pattern";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 임시 Circuit 준비");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            string circuitName = await Temp_Circuit_Scope.OpenAsync();

            try
            {
                Step("GetAllPatterns (빈 Circuit)");
                var req1 = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Get_All_Patterns_Request>();
                var resp = await broker.PublishAndWaitAsync<AB_Get_All_Patterns_Response>(req1, TimeSpan.FromSeconds(5));
                Log("patterns.Count", resp.Data.Count);
                Assert("패턴 0개", resp.Data.Count == 0);
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }
}
