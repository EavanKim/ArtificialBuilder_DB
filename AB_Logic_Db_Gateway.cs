using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core;
using ArtificialBuilder_EDP.Core.Messaging;
using EDPFW;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 활성 Logic DB 에 대한 요청을 메시지 브로커 토픽으로 받아 백엔드 호출로 변환하는 게이트웨이.
    /// 토픽: AB_Logic_Db_Topics.ActiveLogic. EDP 컴포넌트로 등록.
    /// </summary>
    public class AB_Logic_Db_Gateway : ArtificialBuilder_EDP.Core.AB_Component
    {
        private IAB_Message_Broker? m_broker;
        private AB_Subscription_Token? m_sub;

        /// <inheritdoc/>
        public override void OnAttach()
        {
            try
            {
                m_broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
                m_sub = m_broker.Subscribe(AB_Logic_Db_Topics.ActiveLogic, HandleMessage);
            }
            catch (Exception ex)
            {
                AB_Log.Error("LogicGw", $"OnAttach 실패: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public override void OnDetach()
        {
            try
            {
                if (m_broker != null && m_sub != null)
                {
                    m_broker.Unsubscribe(m_sub);
                }
            }
            catch { }
        }

        // TODO(main-tabs-and-package-system sub 2): 로직 라이브러리 Gateway case 추가.
        // AB_Create_Logic_Request → AB_Logic_Db.CreateLogicFile(uuid, name) + Response(uuid).
        // AB_Delete_Logic_Request → AB_Logic_Db.DeleteLogicFile(uuid) + Response(success).
        // AB_Get_Logic_Library_Info_Request → 디렉터리 scan + 각 UUID 의 meta name 추출 + Response.
        // plans/doing/main-tabs-and-package-system/sub-2-logic-library-screen.md
        private async void HandleMessage(AB_Message _msg)
        {
            if (_msg.IsResponse) return;

            try
            {
                int dbId = ActiveDbId;

                switch (_msg)
                {
                    case AB_Get_Logic_Meta_Request req:
                    {
                        AB_Logic_Meta_Model? data = dbId == 0
                            ? null
                            : await AB_Board.Db.GetByIdAsync<AB_Logic_Meta_Model>(dbId, "meta");
                        m_broker?.Publish(new AB_Get_Logic_Meta_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Save_Logic_Meta_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Update(dbId, req.Meta);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Save_Logic_Meta_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_All_Logic_Used_Circuits_Request req:
                    {
                        List<AB_Logic_Used_Circuit_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Logic_Used_Circuit_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Logic_Used_Circuits_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Logic_Used_Circuit_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Logic_Used_Circuit_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Save_Logic_Used_Circuit_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Update(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Save_Logic_Used_Circuit_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Delete_Logic_Used_Circuit_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Remove(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Delete_Logic_Used_Circuit_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_All_Logic_Used_Response_Ui_Request req:
                    {
                        List<AB_Logic_Used_Response_Ui_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Logic_Used_Response_Ui_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Logic_Used_Response_Ui_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Logic_Used_Response_Ui_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Logic_Used_Response_Ui_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Delete_Logic_Used_Response_Ui_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Remove(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Delete_Logic_Used_Response_Ui_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_All_Logic_Sub_Logics_Request req:
                    {
                        List<AB_Logic_Sub_Logic_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Logic_Sub_Logic_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Logic_Sub_Logics_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Logic_Sub_Logic_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Logic_Sub_Logic_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Delete_Logic_Sub_Logic_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Remove(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Delete_Logic_Sub_Logic_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_All_Logic_History_Turns_Request req:
                    {
                        List<AB_Logic_History_Turn_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Logic_History_Turn_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Logic_History_Turns_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Append_Logic_History_Turn_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Append_Logic_History_Turn_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    // --- circuit-home-logic-graph-runtime-db-proxy sub 3 — v2 변수 슬롯 ---
                    case AB_Get_All_Logic_Variable_Slots_Request req:
                    {
                        List<AB_Logic_Variable_Slot_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Logic_Variable_Slot_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Logic_Variable_Slots_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Logic_Variable_Slot_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Logic_Variable_Slot_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Save_Logic_Variable_Slot_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            AB_Board.Db.Update(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Save_Logic_Variable_Slot_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Remove_Logic_Variable_Slot_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            var existing = await AB_Board.Db.GetByIdAsync<AB_Logic_Variable_Slot_Model>(dbId, req.Slot_Id);
                            if (existing != null)
                            {
                                AB_Board.Db.Remove(dbId, existing);
                                await AB_Board.Db.SaveChangesAsync(dbId);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Remove_Logic_Variable_Slot_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_All_Logic_Internal_Nodes_Request req:
                    {
                        List<AB_Logic_Internal_Node_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Logic_Internal_Node_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Logic_Internal_Nodes_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Logic_Internal_Node_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Logic_Internal_Node_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Remove_Logic_Internal_Node_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            var existing = await AB_Board.Db.GetByIdAsync<AB_Logic_Internal_Node_Model>(dbId, req.Node_Id);
                            if (existing != null)
                            {
                                AB_Board.Db.Remove(dbId, existing);
                                await AB_Board.Db.SaveChangesAsync(dbId);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Remove_Logic_Internal_Node_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Get_All_Logic_Internal_Connections_Request req:
                    {
                        List<AB_Logic_Internal_Connection_Model> data = new();
                        if (dbId != 0)
                        {
                            var all = await AB_Board.Db.GetAllAsync<AB_Logic_Internal_Connection_Model>(dbId);
                            data.AddRange(all);
                        }
                        m_broker?.Publish(new AB_Get_All_Logic_Internal_Connections_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }
                    case AB_Add_Logic_Internal_Connection_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            await AB_Board.Db.AddAsync(dbId, req.Item);
                            await AB_Board.Db.SaveChangesAsync(dbId);
                            ok = true;
                        }
                        m_broker?.Publish(new AB_Add_Logic_Internal_Connection_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                    case AB_Remove_Logic_Internal_Connection_Request req:
                    {
                        bool ok = false;
                        if (dbId != 0)
                        {
                            var existing = await AB_Board.Db.GetByIdAsync<AB_Logic_Internal_Connection_Model>(dbId, req.Connection_Id);
                            if (existing != null)
                            {
                                AB_Board.Db.Remove(dbId, existing);
                                await AB_Board.Db.SaveChangesAsync(dbId);
                                ok = true;
                            }
                        }
                        m_broker?.Publish(new AB_Remove_Logic_Internal_Connection_Response
                        { CorrelationId = req.CorrelationId, Success = ok });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AB_Log.Error("LogicGw", $"HandleMessage 실패: {ex.Message}");
            }
        }

        /// <summary>현재 활성 Logic DB 핸들 (0=닫힘).</summary>
        private int ActiveDbId => AB_Board.Logic.Handle;
    }
}
