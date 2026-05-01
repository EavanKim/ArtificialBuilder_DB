using ArtificialBuilder;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Threading.Tasks;

namespace ArtificialBuilder_EDP.Core.Diagnostics
{
    // ─────────────────────────────────────────────────────────────────
    // 10. Gateway_GetAllCharacters_RoundTrip
    // ─────────────────────────────────────────────────────────────────
    /// <summary>
    /// 10. AB_Circuit_Db_Gateway 의 실제 한 사이클 검증.
    ///   브로커 publish → 토픽 구독한 게이트웨이가 백엔드 호출 → 응답 publish → await 깨움.
    /// 임시 진단용 Circuit을 열어 빈 캐릭터 리스트를 받는 경로를 검증.
    /// </summary>
    public class AB_Test_Gateway_Get_All_Characters_Round_Trip : AB_Diagnostic_Test
    {
        /// <inheritdoc/>
        public override string Name => "Gateway_GetAllCharacters_RoundTrip";
        /// <inheritdoc/>
        public override string Category => "Gateway";

        /// <inheritdoc/>
        protected override async Task RunCoreAsync()
        {
            Step("브로커 + 게이트웨이 컴포넌트 참조 획득");
            var broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
            Log("broker.Type", broker.GetType().Name);
            // 게이트웨이 컴포넌트 등록 확인
            try
            {
                var gateway = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_Circuit_Db_Gateway>();
                Log("gateway.Type", gateway.GetType().Name);
                Assert("게이트웨이 등록됨", gateway != null);
            }
            catch (Exception ex)
            {
                Assert("게이트웨이 등록됨", false, ex.Message);
                return;
            }

            Step("진단용 임시 Circuit 열기");
            var circuitDb = ArtificialBuilder_EDP.AB_Board.Circuit;
            string tempName = $"diag_test_{DateTime.UtcNow.Ticks}";
            try
            {
                await circuitDb.OpenAsync(tempName);
                Log("circuit.ActiveName", circuitDb.ActiveName ?? "<null>");
                Assert("Circuit 열림", !string.IsNullOrEmpty(circuitDb.ActiveName));
            }
            catch (Exception ex)
            {
                Assert("Circuit 열기 성공", false, ex.Message);
                return;
            }

            try
            {
                Step("AB_Get_All_Characters_Request 발행 + 응답 대기");
                var req = new AB_Get_All_Characters_Request();
                Log("request.Topic", req.Topic);
                Log("request.IsResponse", req.IsResponse);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                AB_Get_All_Characters_Response resp;
                try
                {
                    resp = await broker.PublishAndWaitAsync<AB_Get_All_Characters_Response>(
                        req, TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    Assert("응답 수신 (예외 없음)", false, $"{ex.GetType().Name}: {ex.Message}");
                    return;
                }
                sw.Stop();

                Step("응답 검증");
                Log("response.IsResponse", resp.IsResponse);
                Log("response.CorrelationId", resp.CorrelationId);
                Log("response.Data.Count", resp.Data?.Count ?? -1);
                Log("response.Error", resp.Error ?? "<null>");
                Log("elapsed", $"{sw.Elapsed.TotalMilliseconds:F1}ms");

                Assert("응답 IsResponse=true", resp.IsResponse);
                Assert("correlation id 일치", resp.CorrelationId == req.CorrelationId);
                Assert("Data 비어 있지 않음(빈 리스트도 OK)", resp.Data != null);
                Assert("에러 없음", resp.Error == null);
                // 신규 Circuit이라 캐릭터 0개 기대
                Assert("신규 Circuit → 캐릭터 0개", (resp.Data?.Count ?? -1) == 0,
                    $"count={resp.Data?.Count}");
            }
            finally
            {
                Step("임시 Circuit 정리");
                try
                {
                    await circuitDb.CloseAsync();
                    // 파일 삭제
                    string path = $"circuit/{tempName}.circuit";
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                        Log("temp_file.deleted", path);
                    }
                }
                catch (Exception ex)
                {
                    Log("cleanup_warning", ex.Message);
                }
            }
        }
    }
}
