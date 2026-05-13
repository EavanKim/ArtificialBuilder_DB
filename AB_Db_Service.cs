using ArtificialBuilder.DDO;
using ArtificialBuilder.Models;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Core;
using System;
using System.Collections.Generic;

namespace ArtificialBuilder
{
    /// <summary>
    /// 5 DB 도메인 단일 진입점 DDO 옵저버 ([[ddo-command-only]] / [[unify-entry-points]] / [[blackboard-data-view]]).
    /// 외부 (Service / Panel / Component) 가 DDO publish 한 명령을 본 Service 가 수신해 AB_Db_Manager 도메인 Proxy 로 라우팅.
    /// 정본: docs/plans/doing/db-three-way-split/5-message-queue.md.
    /// </summary>
    public class AB_Db_Service : AB_Component
    {
        private readonly List<AB_DDO_Observer_Component> m_observers = new();

        public override void OnAttach()
        {
            // --- App DB ---
            AddObs(AB_DB_Command_Type.APP_DB_MODEL_GET_ALL, HandleAppModelGetAll);
            AddObs(AB_DB_Command_Type.APP_DB_MODEL_GET, HandleAppModelGet);
            AddObs(AB_DB_Command_Type.APP_DB_MODEL_ADD, HandleAppModelAdd);
            AddObs(AB_DB_Command_Type.APP_DB_MODEL_SAVE, HandleAppModelSave);
            AddObs(AB_DB_Command_Type.APP_DB_MODEL_DELETE, HandleAppModelDelete);

            // --- Persona DB ---
            AddObs(AB_DB_Command_Type.PERSONA_DB_LOAD_ACTIVE, HandlePersonaLoadActive);

            // --- Circuit DB ---
            AddObs(AB_DB_Command_Type.CIRCUIT_DB_OPEN, HandleCircuitOpen);
            AddObs(AB_DB_Command_Type.CIRCUIT_DB_CLOSE, HandleCircuitClose);
            AddObs(AB_DB_Command_Type.CIRCUIT_DB_USED_SUB_CIRCUIT_GET_ALL, HandleCircuitUsedSubGetAll);
            AddObs(AB_DB_Command_Type.CIRCUIT_DB_USED_SUB_CIRCUIT_ADD, HandleCircuitUsedSubAdd);
            AddObs(AB_DB_Command_Type.CIRCUIT_DB_USED_SUB_CIRCUIT_DELETE, HandleCircuitUsedSubDelete);
            AddObs(AB_DB_Command_Type.CIRCUIT_DB_HOSTED_LOGIC_GET_ALL, HandleCircuitHostedLogicGetAll);
            AddObs(AB_DB_Command_Type.CIRCUIT_DB_HOSTED_LOGIC_ADD, HandleCircuitHostedLogicAdd);
            AddObs(AB_DB_Command_Type.CIRCUIT_DB_HOSTED_LOGIC_DELETE, HandleCircuitHostedLogicDelete);

            // --- Logic DB ---
            AddObs(AB_DB_Command_Type.LOGIC_DB_OPEN, HandleLogicOpen);
            AddObs(AB_DB_Command_Type.LOGIC_DB_CLOSE, HandleLogicClose);
            AddObs(AB_DB_Command_Type.LOGIC_DB_META_GET, HandleLogicMetaGet);
            AddObs(AB_DB_Command_Type.LOGIC_DB_META_SAVE, HandleLogicMetaSave);
            AddObs(AB_DB_Command_Type.LOGIC_DB_USED_CIRCUIT_GET_ALL, HandleLogicUsedCircuitGetAll);
            AddObs(AB_DB_Command_Type.LOGIC_DB_USED_CIRCUIT_ADD, HandleLogicUsedCircuitAdd);
            AddObs(AB_DB_Command_Type.LOGIC_DB_USED_CIRCUIT_SAVE, HandleLogicUsedCircuitSave);
            AddObs(AB_DB_Command_Type.LOGIC_DB_USED_CIRCUIT_DELETE, HandleLogicUsedCircuitDelete);
            AddObs(AB_DB_Command_Type.LOGIC_DB_USED_RESPONSE_UI_GET_ALL, HandleLogicUsedResponseUiGetAll);
            AddObs(AB_DB_Command_Type.LOGIC_DB_USED_RESPONSE_UI_ADD, HandleLogicUsedResponseUiAdd);
            AddObs(AB_DB_Command_Type.LOGIC_DB_USED_RESPONSE_UI_DELETE, HandleLogicUsedResponseUiDelete);
            AddObs(AB_DB_Command_Type.LOGIC_DB_SUB_LOGIC_GET_ALL, HandleLogicSubLogicGetAll);
            AddObs(AB_DB_Command_Type.LOGIC_DB_SUB_LOGIC_ADD, HandleLogicSubLogicAdd);
            AddObs(AB_DB_Command_Type.LOGIC_DB_SUB_LOGIC_DELETE, HandleLogicSubLogicDelete);
            AddObs(AB_DB_Command_Type.LOGIC_DB_HISTORY_GET_ALL, HandleLogicHistoryGetAll);
            AddObs(AB_DB_Command_Type.LOGIC_DB_HISTORY_APPEND, HandleLogicHistoryAppend);

            // --- Response UI DB ---
            AddObs(AB_DB_Command_Type.RESPONSE_UI_DB_OPEN, HandleResponseUiOpen);
            AddObs(AB_DB_Command_Type.RESPONSE_UI_DB_CLOSE, HandleResponseUiClose);
            AddObs(AB_DB_Command_Type.RESPONSE_UI_DB_META_GET, HandleResponseUiMetaGet);
            AddObs(AB_DB_Command_Type.RESPONSE_UI_DB_META_SAVE, HandleResponseUiMetaSave);
            AddObs(AB_DB_Command_Type.RESPONSE_UI_DB_LAYER_GET_ALL, HandleResponseUiLayerGetAll);
            AddObs(AB_DB_Command_Type.RESPONSE_UI_DB_LAYER_ADD, HandleResponseUiLayerAdd);
            AddObs(AB_DB_Command_Type.RESPONSE_UI_DB_LAYER_SAVE, HandleResponseUiLayerSave);
            AddObs(AB_DB_Command_Type.RESPONSE_UI_DB_LAYER_DELETE, HandleResponseUiLayerDelete);
        }

        public override void OnDetach()
        {
            if (AB_Engine.TryGet<AB_DDO_Subscription_Manager>(out var mgr))
            {
                foreach (AB_DDO_Observer_Component obs in m_observers)
                {
                    mgr.UnregisterObserver(obs);
                }
            }
            m_observers.Clear();
        }

        private void AddObs(string _header, Action<AB_DDO_Command> _handler)
        {
            AB_DDO_Observer_Component obs = new();
            obs.Configure(_header, _handler);
            if (AB_Engine.TryGet<AB_DDO_Subscription_Manager>(out var mgr))
            {
                mgr.RegisterObserver(obs);
            }
            m_observers.Add(obs);
        }

        /// <summary>(example-mental-restructure Phase C Sub 3) — typed enum overload.</summary>
        private void AddObs(AB_Object_Command_Type _type, Action<AB_DDO_Command> _handler) => AddObs(AB_DDO_Headers.Get(_type), _handler);
        private void AddObs(AB_Component_Command_Type _type, Action<AB_DDO_Command> _handler) => AddObs(AB_DDO_Headers.Get(_type), _handler);
        private void AddObs(AB_DB_Command_Type _type, Action<AB_DDO_Command> _handler) => AddObs(AB_DDO_Headers.Get(_type), _handler);

        // ================ App DB ================

        private void HandleAppModelGetAll(AB_DDO_Command _cmd)
        {
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().GetAllModelsAsync();
        }

        private void HandleAppModelGet(AB_DDO_Command _cmd)
        {
            if (_cmd.DataKey is not AB_Model_Id modelId) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "AppModelGet — DataKey AB_Model_Id mismatch (ddo-datakey-typed sub 2)"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().GetModelByIdAsync(modelId);
        }

        private void HandleAppModelAdd(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Model_Config_Model model) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "AppModelAdd — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().AddModelAsync(model);
        }

        private void HandleAppModelSave(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Model_Config_Model model) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "AppModelSave — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().UpdateModelAsync(model);
        }

        private void HandleAppModelDelete(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Model_Config_Model model) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "AppModelDelete — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_App_Db_Proxy>().DeleteModelAsync(model);
        }

        // ================ Persona DB ================

        private void HandlePersonaLoadActive(AB_DDO_Command _cmd)
        {
            _ = AB_Board.Persona.LoadActiveAsync();
        }

        // ================ Circuit DB ================

        private void HandleCircuitOpen(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Circuit_Db_Open_Request req || string.IsNullOrEmpty(req.CircuitName_)) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "CircuitOpen — Payload AB_Circuit_Db_Open_Request mismatch (ddo-datakey-typed sub 2)"); return; }
            _ = AB_Board.Circuit.OpenAsync(req.CircuitName_);
        }

        private void HandleCircuitClose(AB_DDO_Command _cmd)
        {
            _ = AB_Board.Circuit.CloseAsync();
        }

        private void HandleCircuitUsedSubGetAll(AB_DDO_Command _cmd)
        {
            _ = QueryAndPublishAsync<AB_Circuit_Used_Sub_Circuit_Model>(_cmd, AB_Board.Circuit.Handle);
        }

        private void HandleCircuitUsedSubAdd(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Circuit_Used_Sub_Circuit_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "CircuitUsedSubAdd — Payload 타입 mismatch"); return; }
            _ = AddAndSaveAsync(AB_Board.Circuit.Handle, item);
        }

        private void HandleCircuitUsedSubDelete(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Circuit_Used_Sub_Circuit_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "CircuitUsedSubDelete — Payload 타입 mismatch"); return; }
            _ = RemoveAndSaveAsync(AB_Board.Circuit.Handle, item);
        }

        private void HandleCircuitHostedLogicGetAll(AB_DDO_Command _cmd)
        {
            _ = QueryAndPublishAsync<AB_Circuit_Hosted_Logic_Model>(_cmd, AB_Board.Circuit.Handle);
        }

        private void HandleCircuitHostedLogicAdd(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Circuit_Hosted_Logic_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "CircuitHostedLogicAdd — Payload 타입 mismatch"); return; }
            _ = AddAndSaveAsync(AB_Board.Circuit.Handle, item);
        }

        private void HandleCircuitHostedLogicDelete(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Circuit_Hosted_Logic_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "CircuitHostedLogicDelete — Payload 타입 mismatch"); return; }
            _ = RemoveAndSaveAsync(AB_Board.Circuit.Handle, item);
        }

        // ================ Logic DB ================

        private void HandleLogicOpen(AB_DDO_Command _cmd)
        {
            if (_cmd.DataKey is not AB_Logic_Id logicId) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "LogicOpen — DataKey AB_Logic_Id mismatch (ddo-datakey-typed sub 2)"); return; }
            _ = AB_Board.Logic.OpenAsync(logicId);
        }

        private void HandleLogicClose(AB_DDO_Command _cmd)
        {
            _ = AB_Board.Logic.CloseAsync();
        }

        private void HandleLogicMetaGet(AB_DDO_Command _cmd)
        {
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().GetMetaAsync();
        }

        private void HandleLogicMetaSave(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Logic_Meta_Model meta) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "LogicMetaSave — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().SaveMetaAsync(meta);
        }

        private void HandleLogicUsedCircuitGetAll(AB_DDO_Command _cmd)
        {
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().GetUsedCircuitsAsync();
        }

        private void HandleLogicUsedCircuitAdd(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Logic_Used_Circuit_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "LogicUsedCircuitAdd — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().AddUsedCircuitAsync(item);
        }

        private void HandleLogicUsedCircuitSave(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Logic_Used_Circuit_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "LogicUsedCircuitSave — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().SaveUsedCircuitAsync(item);
        }

        private void HandleLogicUsedCircuitDelete(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Logic_Used_Circuit_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "LogicUsedCircuitDelete — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().DeleteUsedCircuitAsync(item);
        }

        private void HandleLogicUsedResponseUiGetAll(AB_DDO_Command _cmd)
        {
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().GetUsedResponseUiAsync();
        }

        private void HandleLogicUsedResponseUiAdd(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Logic_Used_Response_Ui_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "LogicUsedResponseUiAdd — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().AddUsedResponseUiAsync(item);
        }

        private void HandleLogicUsedResponseUiDelete(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Logic_Used_Response_Ui_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "LogicUsedResponseUiDelete — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().DeleteUsedResponseUiAsync(item);
        }

        private void HandleLogicSubLogicGetAll(AB_DDO_Command _cmd)
        {
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().GetSubLogicsAsync();
        }

        private void HandleLogicSubLogicAdd(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Logic_Sub_Logic_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "LogicSubLogicAdd — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().AddSubLogicAsync(item);
        }

        private void HandleLogicSubLogicDelete(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Logic_Sub_Logic_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "LogicSubLogicDelete — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().DeleteSubLogicAsync(item);
        }

        private void HandleLogicHistoryGetAll(AB_DDO_Command _cmd)
        {
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().GetHistoryTurnsAsync();
        }

        private void HandleLogicHistoryAppend(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Logic_History_Turn_Model turn) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "LogicHistoryAppend — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Logic_Db_Proxy>().AppendHistoryTurnAsync(turn);
        }

        // ================ Response UI DB ================

        private void HandleResponseUiOpen(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Response_Ui_Db_Open_Request req || string.IsNullOrEmpty(req.Uuid_)) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "ResponseUiOpen — Payload AB_Response_Ui_Db_Open_Request mismatch (ddo-datakey-typed sub 2)"); return; }
            _ = AB_Board.ResponseUi.OpenAsync(req.Uuid_);
        }

        private void HandleResponseUiClose(AB_DDO_Command _cmd)
        {
            _ = AB_Board.ResponseUi.CloseAsync();
        }

        private void HandleResponseUiMetaGet(AB_DDO_Command _cmd)
        {
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Response_Ui_Db_Proxy>().GetMetaAsync();
        }

        private void HandleResponseUiMetaSave(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Response_Ui_Meta_Model meta) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "ResponseUiMetaSave — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Response_Ui_Db_Proxy>().SaveMetaAsync(meta);
        }

        private void HandleResponseUiLayerGetAll(AB_DDO_Command _cmd)
        {
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Response_Ui_Db_Proxy>().GetLayersAsync();
        }

        private void HandleResponseUiLayerAdd(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Response_Ui_Layer_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "ResponseUiLayerAdd — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Response_Ui_Db_Proxy>().AddLayerAsync(item);
        }

        private void HandleResponseUiLayerSave(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Response_Ui_Layer_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "ResponseUiLayerSave — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Response_Ui_Db_Proxy>().SaveLayerAsync(item);
        }

        private void HandleResponseUiLayerDelete(AB_DDO_Command _cmd)
        {
            if (_cmd.Payload is not AB_Response_Ui_Layer_Model item) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", "ResponseUiLayerDelete — Payload 타입 mismatch"); return; }
            _ = global::ArtificialBuilder_EDP.Core.AB_Engine.GetService<AB_Response_Ui_Db_Proxy>().DeleteLayerAsync(item);
        }

        // ================ helpers (Used_Sub_Circuit / Hosted_Logic CRUD — Proxy 미존재) ================

        private static async System.Threading.Tasks.Task QueryAndPublishAsync<T>(AB_DDO_Command _cmd, int _dbId) where T : class, new()
        {
            if (_dbId == 0) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", $"QueryAndPublish<{typeof(T).Name}> — dbId == 0"); return; }
            try
            {
                _ = await AB_Board.Db.GetAllAsync<T>(_dbId);
            }
            catch (Exception ex)
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Error("AB_Db_Service", $"QueryAndPublish<{typeof(T).Name}> 실패: {ex.Message}");
            }
        }

        private static async System.Threading.Tasks.Task AddAndSaveAsync<T>(int _dbId, T _item) where T : class
        {
            if (_dbId == 0) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", $"AddAndSave<{typeof(T).Name}> — dbId == 0"); return; }
            try
            {
                await AB_Board.Db.AddAsync(_dbId, _item);
                await AB_Board.Db.SaveChangesAsync(_dbId);
            }
            catch (Exception ex)
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Error("AB_Db_Service", $"AddAndSave<{typeof(T).Name}> 실패: {ex.Message}");
            }
        }

        private static async System.Threading.Tasks.Task RemoveAndSaveAsync<T>(int _dbId, T _item) where T : class
        {
            if (_dbId == 0) { ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Warn("AB_Db_Service", $"RemoveAndSave<{typeof(T).Name}> — dbId == 0"); return; }
            try
            {
                AB_Board.Db.Remove(_dbId, _item);
                await AB_Board.Db.SaveChangesAsync(_dbId);
            }
            catch (Exception ex)
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Error("AB_Db_Service", $"RemoveAndSave<{typeof(T).Name}> 실패: {ex.Message}");
            }
        }
    }
}
