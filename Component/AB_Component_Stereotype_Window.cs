using System;
using System.Collections.Generic;
using ArtificialBuilder;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Data;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Component
{
    // 단순형 stereotype sliding window caller-side helper Component.
    // storage-policy 2026-05-17 § "Active Result 추적 정책" 단순형 정합.
    //
    // BuildWindow_(current_turn_id, output_slot_id) 호출 = sync. 내부 N 회 loop 매개:
    //   1) m_pending = current_turn_id, shard_key = m_pending / 100
    //   2) Turn GET_BY_ID enqueue + AB_Manager_DB.DrainQueue() 동기 호출 → 내부 칠판 slot Turn_Row set
    //   3) SelectedIndex / PrevTurnId 추출
    //   4) Result FIND_BY_TURN enqueue + DrainQueue → 내부 칠판 slot Node_List set
    //   5) list 안 ResultSeq == SelectedIndex row 추출 → collected.Add
    //   6) remaining--, m_pending = PrevTurnId. > 0 + != 0 = step 2 / 외 = 종결
    //   7) output_slot_id 매개 AB_Data_Stereotype_Window slot 안 collected 복제 + NotifyDataKey
    // 종결 후 내부 칠판 slot 4 (Turn_Row / Node_List / 입력 long 2) Unregister.
    // 옵저버 wiring / state machine 불필요 — sync DrainQueue 호출 매개 1 BuildWindow_ call 안 묶음 완성.
    public class AB_Component_Stereotype_Window : AB_Component
    {
        // window size N — default 10 (storage-policy 정본 ~10 매개).
        private int m_window_size;

        public AB_Component_Stereotype_Window()
        {
            m_window_size = 10;
        }

        public void SetWindowSize_(int _window_size)
        {
            if (_window_size <= 0)
            {
                throw new InvalidOperationException("AB_Component_Stereotype_Window.SetWindowSize_: window_size <= 0 위반 value=" + _window_size);
            }
            m_window_size = _window_size;
        }

        public int WindowSize_ => m_window_size;

        // sliding window 구성. sync — 종결 시점 _output_slot_id 매개 AB_Data_Stereotype_Window slot set + NotifyDataKey 발화.
        public void BuildWindow_(long _current_turn_id, long _output_slot_id)
        {
            AB_Object_Blackboard blackboard = AB_Service.Get<AB_Object_Blackboard>();
            AB_Manager_DB db_manager = AB_Service.Get<AB_Manager_DB>();

            long turn_slot = blackboard.Register<AB_Data_DB_Turn_Row>(new AB_Data_DB_Turn_Row(), null);
            long result_slot = blackboard.Register<AB_Data_DB_Node_List>(new AB_Data_DB_Node_List(), null);
            long turn_input_slot = blackboard.Register<AB_Data_Long>(new AB_Data_Long(), null);
            long result_input_slot = blackboard.Register<AB_Data_Long>(new AB_Data_Long(), null);

            List<AB_Object_DB_Node_Row> collected = new List<AB_Object_DB_Node_Row>();
            long pending = _current_turn_id;
            int remaining = m_window_size;

            try
            {
                while (remaining > 0 && pending != 0)
                {
                    long shard_key = pending / 100;

                    // step 1 — Turn GET_BY_ID (sync)
                    blackboard.Lookup<AB_Data_Long>(turn_input_slot).Set(pending);
                    db_manager.EnqueueRequest(AB_DB_Turn_Command_Type.GET_BY_ID, turn_input_slot, turn_slot, shard_key);
                    db_manager.DrainQueue();

                    AB_Object_DB_Turn_Row? turn_row = blackboard.Lookup<AB_Data_DB_Turn_Row>(turn_slot).Get();
                    if (turn_row == null)
                    {
                        break;
                    }

                    int selected_index = turn_row.SelectedIndex;
                    long prev_turn_id = turn_row.PrevTurnId ?? 0L;

                    // step 2 — Result FIND_BY_TURN (sync)
                    blackboard.Lookup<AB_Data_Long>(result_input_slot).Set(pending);
                    db_manager.EnqueueRequest(AB_DB_Result_Command_Type.FIND_BY_TURN, result_input_slot, result_slot, shard_key);
                    db_manager.DrainQueue();

                    List<AB_Object_DB_Node_Row> node_list = blackboard.Lookup<AB_Data_DB_Node_List>(result_slot).Get();
                    foreach (AB_Object_DB_Node_Row row in node_list)
                    {
                        if (row.ResultSeq == selected_index)
                        {
                            collected.Add(row);
                            break;
                        }
                    }

                    remaining--;
                    pending = prev_turn_id;
                }

                blackboard.Lookup<AB_Data_Stereotype_Window>(_output_slot_id).Set(collected);
                blackboard.NotifyDataKey(_output_slot_id);
            }
            finally
            {
                blackboard.Unregister(turn_slot);
                blackboard.Unregister(result_slot);
                blackboard.Unregister(turn_input_slot);
                blackboard.Unregister(result_input_slot);
            }
        }

        public override void Dispose()
        {
        }
    }
}
