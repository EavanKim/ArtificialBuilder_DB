using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtificialBuilder_EDP.Core;

namespace ArtificialBuilder
{
    /// <summary>
    /// 파이프라인 디버그 DB 프록시. 브로커 + AB_Pipeline_Debug_Db_Gateway 경유.
    /// UI/컴포넌트 계층의 단일 진입점.
    /// </summary>
    public class AB_Pipeline_Debug_Db_Proxy : ArtificialBuilder_EDP.Core.AB_Object
    {
        private TimeSpan m_defaultTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan DefaultTimeout
        {
            get { return m_defaultTimeout; }
            set { m_defaultTimeout = value; }
        }

        private IAB_Message_Broker GetBroker()
            => ArtificialBuilder_EDP.Core.AB_Engine.Get<AB_In_Memory_Broker>();

        // ============================================================
        // 배치 기록 (fire-and-forget, 응답은 대기만 ack 용)
        // ============================================================

        /// <summary>디버그 엔트리 배치 기록. 빈 리스트면 즉시 반환.</summary>
        public async Task WriteBatchAsync(IReadOnlyList<AB_Pipeline_Debug_Entry_Model> _entries)
        {
            if (_entries == null || _entries.Count == 0) return;
            var list = new List<AB_Pipeline_Debug_Entry_Model>(_entries);
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Write_Pipeline_Debug_Batch_Request>();
            req.Entries = list;
            await GetBroker().PublishAndWaitAsync<AB_Write_Pipeline_Debug_Batch_Response>(req, DefaultTimeout);
        }

        // ============================================================
        // 페이징 조회
        // ============================================================

        /// <summary>세션 기준 페이징 조회.</summary>
        public async Task<List<AB_Pipeline_Debug_Entry_Model>> QueryAsync(
            long _sessionId, string? _entryType = null, int _offset = 0, int _limit = 50)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Query_Pipeline_Debug_Request>();
            req.SessionId = _sessionId;
            req.EntryType = _entryType;
            req.Offset = _offset;
            req.Limit = _limit;
            var resp = await GetBroker().PublishAndWaitAsync<AB_Query_Pipeline_Debug_Response>(req, DefaultTimeout);
            return resp.Data ?? new();
        }

        // ============================================================
        // 보존 정리
        // ============================================================

        /// <summary>보존 기한 초과 엔트리 정리.</summary>
        public async Task RetentionSweepAsync(int _retentionDays)
        {
            var req = AB_Engine.GetService<AB_Pool>().AcquireObject<AB_Retention_Sweep_Pipeline_Debug_Request>();
            req.RetentionDays = _retentionDays;
            await GetBroker().PublishAndWaitAsync<AB_Retention_Sweep_Pipeline_Debug_Response>(req, DefaultTimeout);
        }
    }
}
