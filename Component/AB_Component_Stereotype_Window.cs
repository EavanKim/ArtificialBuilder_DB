using System;
using System.Collections.Generic;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Component
{
    // 단순형 stereotype sliding window caller-side helper Component.
    // storage-policy 2026-05-17 § "Active Result 추적 정책" 단순형 정합.
    //
    // 본 Component = caller (AI provider Object 등) 매개 attach 매개 사용. caller 측 매개 enqueue chain + 칠판 옵저버 매개 결과 묶음 구성.
    // round 1 (2026-05-17) = skeleton (필드 / ctor / API 시그니처 / 본체 NotImplementedException stub). round 2 = state machine + 옵저버 wiring 본체.
    //
    // 동작 알고리즘 (round 2 매개 본체) — storage-policy.md § "단순형 stereotype 사용법" 정합:
    //   1) BuildWindow_(current_turn_id, output_slot_id) 호출 시점 = state init (m_remaining = m_window_size / m_collected.Clear / m_target_slot_id = output_slot_id / m_pending_turn_id = current_turn_id)
    //   2) shard_key = m_pending_turn_id / 100 매개 EnqueueRequest(AB_DB_Turn_Command_Type.GET_BY_ID, ...) → 내부 칠판 slot Turn_Row 매개 set
    //   3) 옵저버 callback (Turn_Row arrived) → SelectedIndex / PrevTurnId 추출 → shard_key = m_pending_turn_id / 100 매개 EnqueueRequest(AB_DB_Result_Command_Type.FIND_BY_TURN, ...) → 내부 칠판 slot Node_List 매개 set
    //   4) 옵저버 callback (Node_List arrived) → list 안 ResultSeq == SelectedIndex row 추출 → m_collected.Add → m_remaining-- → m_pending_turn_id = PrevTurnId
    //   5) m_remaining > 0 + m_pending_turn_id != 0 (null = 0 sentinel) = step 2 재진입 / 외 = 종결
    //   6) 종결 = output_slot_id 매개 AB_Data_Stereotype_Window slot 안 m_collected 복제 + Subscribe 해제
    //
    // 비-async wrap (BuildWindow_ 호출 자체 = sync, 결과 = 칠판 매개 async 도착) — caller = NotifyDataKey 옵저버 매개 output_slot_id 도착 시점 매개 read.
    public class AB_Component_Stereotype_Window : AB_Component
    {
        // round 1 (skeleton) — 본 필드 5 (m_pending_turn_id / m_remaining / m_target_slot_id / m_internal_turn_slot_id / m_internal_result_slot_id) = round 2 state machine 매개 read 진입. round 1 = ctor 매개 init만. CS0414 warning 매개 pragma 차단.
#pragma warning disable CS0414
        // window size N — default 10 (storage-policy 정본 ~10 매개). setter 매개 caller 조정 가능 (S3=b).
        private int m_window_size;

        // BuildWindow_ 진행 중 진입 turn id. step 2/3/4 매개 진행 + 종결 시 0 sentinel.
        private long m_pending_turn_id;

        // N 회 backward chain 남은 카운트. step 4 매개 -- + 0 시 종결.
        private int m_remaining;

        // active result row 누적 — 최근 → 과거 순서. 종결 시 output_slot_id 매개 복제.
        private readonly List<AB_Object_DB_Node_Row> m_collected;

        // 결과 출력 slot DataId — BuildWindow_ 인자 매개 caller 결정. 종결 시 본 slot 매개 list set.
        private long m_target_slot_id;

        // step 2 (Turn GET_BY_ID) 결과 도착 내부 칠판 slot DataId. round 2 = ctor 매개 pre-reserved + Subscribe.
        // round 1 = 미사용 (skeleton).
        private long m_internal_turn_slot_id;

        // step 3 (Result FIND_BY_TURN) 결과 도착 내부 칠판 slot DataId. round 2 = ctor 매개 pre-reserved + Subscribe.
        // round 1 = 미사용 (skeleton).
        private long m_internal_result_slot_id;
#pragma warning restore CS0414

        public AB_Component_Stereotype_Window()
        {
            m_window_size = 10;
            m_pending_turn_id = 0;
            m_remaining = 0;
            m_collected = new List<AB_Object_DB_Node_Row>();
            m_target_slot_id = 0;
            m_internal_turn_slot_id = 0;
            m_internal_result_slot_id = 0;
        }

        // window size setter — caller 매개 조정 (S3=b).
        public void SetWindowSize_(int _window_size)
        {
            if (_window_size <= 0)
            {
                throw new InvalidOperationException("AB_Component_Stereotype_Window.SetWindowSize_: window_size <= 0 위반 value=" + _window_size);
            }
            m_window_size = _window_size;
        }

        public int WindowSize_ => m_window_size;

        // sliding window 구성 진입. 본 호출 자체 = sync (state init + 첫 enqueue). 결과 = output_slot_id 매개 async 도착.
        // _current_turn_id = chain 시작 turn id (= caller 측 현재 turn). _output_slot_id = 결과 List<NodeRow> set 받을 AB_Data_Stereotype_Window slot DataId.
        // round 1 = stub. round 2 = state machine 본체 + 매니저 enqueue chain + 칠판 옵저버 wiring.
        public void BuildWindow_(long _current_turn_id, long _output_slot_id)
        {
            throw new NotImplementedException("AB_Component_Stereotype_Window.BuildWindow_: round 2 매개 본체 신설. round 1 = skeleton stub");
        }

        public override void Dispose()
        {
            m_collected.Clear();
            m_pending_turn_id = 0;
            m_remaining = 0;
            m_target_slot_id = 0;
            m_internal_turn_slot_id = 0;
            m_internal_result_slot_id = 0;
        }
    }
}
