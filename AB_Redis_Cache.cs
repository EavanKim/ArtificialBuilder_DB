using ArtificialBuilder_EDP;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// Redis 기반 캐시 구현. 현재 프로젝트 내 활성 사용처 미확인 (의존성만 등록).
    /// </summary>
    public class AB_Redis_Cache : ArtificialBuilder_EDP.Core.AB_Object, IDb_Cache
    {
        /// <inheritdoc/>
        public string CacheName => "Redis";
        /// <inheritdoc/>
        public bool IsConnected => m_connection?.IsConnected == true;

        private ConnectionMultiplexer? m_connection;
        private IDatabase? m_db;

        /// <inheritdoc/>
        public async Task InitializeAsync(string _connectionString)
        {
            try
            {
                m_connection = await ConnectionMultiplexer.ConnectAsync(_connectionString);
                m_db = m_connection.GetDatabase();
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("Redis", $"연결 성공: {_connectionString}");
            }
            catch (Exception ex)
            {
                ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Error("Redis", $"연결 실패: {ex.Message}");
                m_connection = null;
                m_db = null;
            }
        }

        /// <inheritdoc/>
        public async Task<string?> GetAsync(string _key)
        {
            if (m_db == null) return null;
            var val = await m_db.StringGetAsync(_key);
            return val.HasValue ? val.ToString() : null;
        }

        /// <inheritdoc/>
        public async Task SetAsync(string _key, string _value, TimeSpan? _expiry = null)
        {
            if (m_db == null) return;
            await m_db.StringSetAsync(_key, _value, _expiry);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(string _key)
        {
            if (m_db == null) return;
            await m_db.KeyDeleteAsync(_key);
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string _key)
        {
            if (m_db == null) return false;
            return await m_db.KeyExistsAsync(_key);
        }

        /// <inheritdoc/>
        public async Task FlushAsync()
        {
            if (m_connection == null) return;
            var server = m_connection.GetServer(m_connection.GetEndPoints()[0]);
            await server.FlushDatabaseAsync();
        }

        /// <inheritdoc/>
        public new void Dispose()
        {
            m_connection?.Dispose();
            m_connection = null;
            m_db = null;
        }
    }
}
