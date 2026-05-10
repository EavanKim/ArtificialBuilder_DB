using ArtificialBuilder.Models;
using ArtificialBuilder.Requests;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Components;
using ArtificialBuilder_EDP.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 파이프라인 디버그 DB 프록시. 브로커 + AB_Pipeline_Debug_Db_Gateway 경유.
    /// UI/컴포넌트 계층의 단일 진입점.
    /// </summary>
    public class AB_Pipeline_Debug_Db_Proxy
    {
        private static AB_Pipeline_Debug_Db_Proxy? g_instance;
        /// <summary>전역 단일 인스턴스.</summary>
        public static AB_Pipeline_Debug_Db_Proxy I => g_instance ??= new AB_Pipeline_Debug_Db_Proxy();

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
            await GetBroker().PublishAndWaitAsync<AB_Write_Pipeline_Debug_Batch_Response>(
                new AB_Write_Pipeline_Debug_Batch_Request { Entries = list }, DefaultTimeout);
        }

        // ============================================================
        // 페이징 조회
        // ============================================================

        /// <summary>세션 기준 페이징 조회.</summary>
        public async Task<List<AB_Pipeline_Debug_Entry_Model>> QueryAsync(
            long _sessionId, string? _entryType = null, int _offset = 0, int _limit = 50)
        {
            var resp = await GetBroker().PublishAndWaitAsync<AB_Query_Pipeline_Debug_Response>(
                new AB_Query_Pipeline_Debug_Request
                {
                    SessionId = _sessionId,
                    EntryType = _entryType,
                    Offset = _offset,
                    Limit = _limit
                }, DefaultTimeout);
            return resp.Data ?? new();
        }

        // ============================================================
        // 보존 정리
        // ============================================================

        /// <summary>보존 기한 초과 엔트리 정리.</summary>
        public async Task RetentionSweepAsync(int _retentionDays)
        {
            await GetBroker().PublishAndWaitAsync<AB_Retention_Sweep_Pipeline_Debug_Response>(
                new AB_Retention_Sweep_Pipeline_Debug_Request { RetentionDays = _retentionDays }, DefaultTimeout);
        }
    }
}
