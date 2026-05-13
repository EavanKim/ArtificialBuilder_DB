using ArtificialBuilder;
using ArtificialBuilder_EDP.Core;
using EDPFW;
using System.Collections.Generic;

namespace ArtificialBuilder_EDP.Components
{
    /// <summary>
    /// pending_deletions 큐를 매 틱 정리하는 sweeper. leaf-first time-budgeted.
    ///
    /// 동작:
    ///  - 매 틱 SweepDeletionStepAsync(BUDGET_MICROS) 호출
    ///  - 큐 비어있으면 (drained) 일정 틱 동안 idle (조용한 절전)
    ///  - 새 enqueue 가 들어오면 다시 active
    ///
    /// budget 은 8ms 가량 — UI 입력 / 파이프라인 틱 방해 최소화. 큰 cascade 가 와도 N 틱에 걸쳐 분할.
    /// </summary>
    public class AB_Deletion_Sweeper_Component : AB_Component
    {
        /// <summary>매 틱 sweep budget (microseconds). 한 sweep 호출 안에서 가능한 만큼 leaf 삭제.</summary>
        private const long g_BUDGET_MICROS = 8_000;

        /// <summary>큐가 비어 있을 때 다음 시도까지 skip 할 틱 수 (절전).</summary>
        private const int g_IDLE_SKIP_TICKS = 60;

        private int m_idleSkip;
        private bool m_inFlight;

        public override IEnumerator<EDP_Coroutine_State> run(double _deltaSec)
        {
            if (m_inFlight) yield break;
            if (m_idleSkip > 0)
            {
                m_idleSkip--;
                yield break;
            }

            m_inFlight = true;
            _ = RunSweepStepAsync();
            yield break;
        }

        private async System.Threading.Tasks.Task RunSweepStepAsync()
        {
            try
            {
                var (rowsDeleted, drained) = await global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Storage_Proxy>().SweepDeletionStepAsync(g_BUDGET_MICROS);
                if (drained)
                    m_idleSkip = g_IDLE_SKIP_TICKS;
                else if (rowsDeleted > 0)
                    ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Debug("DeletionSweeper", $"sweep — rowsDeleted={rowsDeleted} budget={g_BUDGET_MICROS}μs (큐 잔여)");
            }
            catch (System.Exception ex)
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("DeletionSweeper", $"sweep step 실패: {ex.Message}");
                m_idleSkip = g_IDLE_SKIP_TICKS;
            }
            finally
            {
                m_inFlight = false;
            }
        }
    }
}
