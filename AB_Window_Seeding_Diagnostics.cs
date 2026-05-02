using ArtificialBuilder;
using ArtificialBuilder.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder_EDP.Core.Diagnostics
{
    /// <summary>
    /// AB_Window_Component_Seeding 통합 테스트 — 빈 윈도우 원칙 + 멱등성 검증.
    /// [[project_response_window_composition]] 원칙: Window 는 범용 컨테이너, 성격은 컴포넌트로 결정.
    /// EnsureDefaultComponentsAsync 가 Frame/Layout/Depth 만 시드하고 message 자동 부착 안 하는지 확인.
    /// </summary>
    public class AB_Test_Win_Seed_Empty_Window : AB_Diagnostic_Test
    {
        public override string Name => "WinSeed_EmptyWindow";
        public override string Category => "WinSeed";

        protected override async Task RunCoreAsync()
        {
            Step("임시 Circuit");
            string circuitName = await Temp_Circuit_Scope.OpenAsync();
            try
            {
                Step("빈 DB 윈도우 생성 (컴포넌트 0)");
                var win = new AB_Response_Window_Model { Name_ = "test_empty" };
                await AB_Circuit_Db_Proxy.I.AddWindowAsync(win);

                Step("EnsureDefaultComponentsAsync 호출");
                await AB_Window_Component_Seeding.EnsureDefaultComponentsAsync();

                Step("window_components 조회 + 구성 검증");
                List<AB_Window_Component_Model> comps = await AB_Circuit_Db_Proxy.I.GetAllWindowComponentsAsync();
                List<AB_Window_Component_Model> forWin = new();
                foreach (var c in comps)
                {
                    if (c.WindowId_ == win.Id_) forWin.Add(c);
                }
                Log("components.Count", forWin.Count);

                int frameCount = 0, layoutCount = 0, depthCount = 0, messageCount = 0, otherCount = 0;
                foreach (var c in forWin)
                {
                    switch (c.ComponentType_)
                    {
                        case "frame": frameCount++; break;
                        case "layout": layoutCount++; break;
                        case "depth": depthCount++; break;
                        case "message": messageCount++; break;
                        default: otherCount++; break;
                    }
                }

                Assert("Frame 1 개", frameCount == 1, $"got {frameCount}");
                Assert("Layout 1 개", layoutCount == 1, $"got {layoutCount}");
                Assert("Depth 1 개", depthCount == 1, $"got {depthCount}");
                Assert("Message 자동 부착 없음 (빈 윈도우 원칙)", messageCount == 0, $"got {messageCount} — EnsureDefaultComponentsAsync 가 message 를 자동 부착하면 빈 윈도우 원칙 위배");
                Assert("기타 성격 컴포넌트 없음", otherCount == 0, $"got {otherCount}");
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }

    /// <summary>
    /// AB_Window_Component_Seeding.ApplyCircuitDefAsync 멱등성 — 같은 Circuit 타입으로 두 번 호출 시 윈도우 중복 생성 없음.
    /// FindWindowByNameAsync 가 이름 중복을 재사용하는지 확인.
    /// </summary>
    public class AB_Test_Win_Seed_Apply_Idempotent : AB_Diagnostic_Test
    {
        public override string Name => "WinSeed_ApplyIdempotent";
        public override string Category => "WinSeed";

        protected override async Task RunCoreAsync()
        {
            Step("임시 Circuit");
            string circuitName = await Temp_Circuit_Scope.OpenAsync();
            try
            {
                Step("1차 ApplyCircuitDefAsync(chat)");
                var applied1 = await AB_Window_Component_Seeding.ApplyCircuitDefAsync("chat");
                int count1 = applied1.Windows.Count;
                Log("1차.Windows.Count", count1);
                Assert("1차 결과 4 윈도우 (chat.json — back + 3)", count1 == 4, $"got {count1}");

                List<AB_Response_Window_Model> dbAfter1 = await AB_Circuit_Db_Proxy.I.GetAllWindowsAsync();
                Log("DB.windows.Count 1차 후", dbAfter1.Count);

                Step("2차 ApplyCircuitDefAsync(chat) — 멱등 확인");
                var applied2 = await AB_Window_Component_Seeding.ApplyCircuitDefAsync("chat");
                Log("2차.Windows.Count", applied2.Windows.Count);

                List<AB_Response_Window_Model> dbAfter2 = await AB_Circuit_Db_Proxy.I.GetAllWindowsAsync();
                Log("DB.windows.Count 2차 후", dbAfter2.Count);
                Assert("DB 중복 생성 없음", dbAfter2.Count == dbAfter1.Count, $"1차 {dbAfter1.Count} → 2차 {dbAfter2.Count}, 재호출이 중복 생성하면 멱등 위반");

                Step("primary chat window 동일");
                Assert("PrimaryChatWindowId 동일", applied1.PrimaryChatWindowId == applied2.PrimaryChatWindowId,
                    $"1차 {applied1.PrimaryChatWindowId} vs 2차 {applied2.PrimaryChatWindowId}");
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }

    /// <summary>
    /// AB_Window_Component_Seeding.EnvelopeMatchesDb 순수 함수 정합성 — 실 envelope 와 DB 를 동시에 만든 뒤
    /// ApplyCircuitDefAsync 의 EnvelopeJson 이 DB windowId 와 매칭되는지 검증 (envelope stale 아님).
    /// </summary>
    public class AB_Test_Win_Seed_Envelope_Consistency : AB_Diagnostic_Test
    {
        public override string Name => "WinSeed_EnvelopeConsistency";
        public override string Category => "WinSeed";

        protected override async Task RunCoreAsync()
        {
            Step("임시 Circuit");
            string circuitName = await Temp_Circuit_Scope.OpenAsync();
            try
            {
                Step("ApplyCircuitDefAsync(chat)");
                var applied = await AB_Window_Component_Seeding.ApplyCircuitDefAsync("chat");

                Step("DB windowIds");
                List<AB_Response_Window_Model> db = await AB_Circuit_Db_Proxy.I.GetAllWindowsAsync();
                var ids = new List<string>();
                foreach (var w in db) ids.Add(w.Id_);
                Log("db.windowIds", string.Join(",", ids));

                Step("envelope 이 DB 와 매칭");
                bool matches = AB_Window_Component_Seeding.EnvelopeMatchesDb(applied.EnvelopeJson, ids);
                Assert("matches == true (신규 생성 직후 stale 아님)", matches, "방금 만든 envelope 이 DB 와 매칭되지 않으면 ApplyCircuitDefAsync 가 ID 를 envelope 에 잘못 기록한 것");

                Step("가짜 DB 셋과 매칭 시 stale 검출");
                bool stale = AB_Window_Component_Seeding.EnvelopeMatchesDb(applied.EnvelopeJson, new[] { "bogus1", "bogus2" });
                Assert("가짜 셋은 stale (false)", !stale, "EnvelopeMatchesDb 가 무관한 ID 셋을 true 로 판정하면 잘못된 매칭");
            }
            finally
            {
                Step("임시 Circuit 정리");
                await Temp_Circuit_Scope.CloseAndDeleteAsync(circuitName);
            }
        }
    }
}
