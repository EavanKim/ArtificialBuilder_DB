using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using EDPFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 파이프라인 디버그 DB 게이트웨이. 브로커 토픽 db.pipelinedebug 구독,
    /// 전용 pipeline_debug.pdb 파일을 OnAttach 에서 열고 EDP_Db_Engine 직결 호출.
    /// DB 핸들 0 인 경우(미초기화/오픈 실패) 빈/false 응답.
    /// </summary>
    public class AB_Pipeline_Debug_Db_Gateway : ArtificialBuilder_EDP.Core.AB_Component
    {
        private IAB_Message_Broker? m_broker;
        private AB_Subscription_Token? m_sub;
        private readonly AB_Pipeline_Debug_Db m_db = new();
        private EDP_Db_Engine m_engine => AB_Board.Db;

        /// <summary>
        /// 내부 DB 인스턴스 — 게이트웨이가 수명을 관리한다. 외부 직접 호출은 금지
        /// (모든 접근은 AB_Pipeline_Debug_Db_Proxy 경유).
        /// </summary>
        public AB_Pipeline_Debug_Db Db => m_db;

        /// <summary>m_db.Initialize + Open 이 1 회만 수행되도록 가드. lazy 초기화 — 첫 message 또는 AB_Board.Bind 시점 이후로 미룸 (OnAttach 시 AB_Board.Db 아직 null).</summary>
        private bool m_dbInitialized;

        /// <inheritdoc/>
        public override void OnAttach()
        {
            try
            {
                // m_db.Initialize 는 lazy — OnAttach 시점은 AB_Board.Bind (Program.cs InitializeAll 후반) 이전이라 AB_Board.Db == null. NRE 회피.
                m_broker = ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();
                m_sub = m_broker.Subscribe(AB_Pipeline_Debug_Db_Topics.PipelineDebug, HandleMessage);
            }
            catch (Exception ex)
            {
                AB_Log.Error("PipelineDebugGw", $"OnAttach 실패: {ex.Message}");
            }
        }

        /// <summary>m_db 초기화 + 기본 파일 열기. 첫 message handle 시 lazy 호출. AB_Board.Db 가 비-null 인 시점 보장.</summary>
        private async Task EnsureDbInitializedAsync()
        {
            if (m_dbInitialized) return;
            if (AB_Board.Db == null)
            {
                AB_Log.Warn("PipelineDebugGw", "AB_Board.Db 미 bind — DB 초기화 보류");
                return;
            }
            m_db.Initialize(m_engine);
            await m_db.OpenDefaultAsync();
            m_dbInitialized = true;
        }

        /// <inheritdoc/>
        public override void OnDetach()
        {
            try
            {
                if (m_broker != null && m_sub != null)
                    m_broker.Unsubscribe(m_sub);
            }
            catch { }

            try
            {
                m_db.CloseAsync().GetAwaiter().GetResult();
            }
            catch { }
        }

        private int Handle => m_db.Handle;

        private async void HandleMessage(AB_Message _msg)
        {
            if (_msg.IsResponse) return;

            try
            {
                await EnsureDbInitializedAsync();
                int handle = Handle;

                switch (_msg)
                {
                    // ============================================================
                    // 배치 기록
                    // ============================================================
                    case AB_Write_Pipeline_Debug_Batch_Request req:
                    {
                        int written = 0;
                        if (handle != 0 && req.Entries != null && req.Entries.Count > 0)
                        {
                            foreach (var entry in req.Entries)
                                await m_engine.AddAsync(handle, entry);
                            await m_engine.SaveChangesAsync(handle);
                            written = req.Entries.Count;
                        }
                        m_broker?.Publish(new AB_Write_Pipeline_Debug_Batch_Response
                        { CorrelationId = req.CorrelationId, Success = handle != 0, Written = written });
                        break;
                    }

                    // ============================================================
                    // 페이징 조회
                    // ============================================================
                    case AB_Query_Pipeline_Debug_Request req:
                    {
                        List<AB_Pipeline_Debug_Entry_Model> data = new();
                        if (handle != 0)
                        {
                            long sid = req.SessionId;
                            IEnumerable<AB_Pipeline_Debug_Entry_Model> all;
                            if (req.EntryType != null)
                                all = await m_engine.FindAsync<AB_Pipeline_Debug_Entry_Model>(handle,
                                    _e => _e.SessionId_ == sid && _e.EntryType_ == req.EntryType);
                            else
                                all = await m_engine.FindAsync<AB_Pipeline_Debug_Entry_Model>(handle,
                                    _e => _e.SessionId_ == sid);

                            var sorted = new List<AB_Pipeline_Debug_Entry_Model>(all);
                            int CompareDesc(AB_Pipeline_Debug_Entry_Model _a, AB_Pipeline_Debug_Entry_Model _b) => _b.TimestampUtc_.CompareTo(_a.TimestampUtc_);
                            sorted.Sort(CompareDesc);

                            int skip = Math.Min(req.Offset, sorted.Count);
                            int take = Math.Min(req.Limit, sorted.Count - skip);
                            if (take > 0) data = sorted.GetRange(skip, take);
                        }
                        m_broker?.Publish(new AB_Query_Pipeline_Debug_Response
                        { CorrelationId = req.CorrelationId, Data = data });
                        break;
                    }

                    // ============================================================
                    // 보존 정리 (현재 EDP_Db_Engine 삭제 API 부재 — ack 만 반환)
                    // ============================================================
                    case AB_Retention_Sweep_Pipeline_Debug_Request req:
                    {
                        // 향후 raw SQL 또는 엔진 삭제 API 확장 시 여기에 구현.
                        m_broker?.Publish(new AB_Retention_Sweep_Pipeline_Debug_Response
                        { CorrelationId = req.CorrelationId, Success = true });
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AB_Log.Error("PipelineDebugGw", $"HandleMessage 실패: {ex.Message}");
            }
        }
    }
}
