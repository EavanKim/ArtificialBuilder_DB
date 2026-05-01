using System;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 외부 DB 백엔드 (연결 문자열 기반).
    /// EDP_Db_Engine의 OpenDatabase를 외부 연결 문자열로 사용.
    /// </summary>
    public class AB_External_Db_Backend : AB_Db_Backend_Base
    {
        /// <inheritdoc/>
        public override string BackendName => $"External ({m_connectionString?[..Math.Min(30, m_connectionString?.Length ?? 0)]})";

        private string? m_connectionString;

        /// <inheritdoc/>
        public override Task InitializeAsync(string _connectionString)
        {
            m_connectionString = _connectionString;
            return Task.CompletedTask;
        }
    }
}
