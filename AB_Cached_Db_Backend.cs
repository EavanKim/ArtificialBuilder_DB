using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// IAB_Db_Backend + IDb_Cache 합성.
    /// 읽기 시 캐시 우선, 쓰기 시 DB + 캐시 동시 갱신.
    /// </summary>
    public class AB_Cached_Db_Backend : IAB_Db_Backend
    {
        /// <inheritdoc/>
        public string BackendName => $"{m_backend.BackendName} + {m_cache.CacheName}";
        /// <inheritdoc/>
        public bool IsConnected => m_backend.IsConnected;

        private readonly IAB_Db_Backend m_backend;
        private readonly IDb_Cache m_cache;
        private readonly TimeSpan m_defaultExpiry;

        /// <summary>백엔드 + 캐시 + 기본 만료시간(기본 10분)으로 생성.</summary>
        public AB_Cached_Db_Backend(IAB_Db_Backend _backend, IDb_Cache _cache, TimeSpan? _defaultExpiry = null)
        {
            m_backend = _backend;
            m_cache = _cache;
            m_defaultExpiry = _defaultExpiry ?? TimeSpan.FromMinutes(10);
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(string _connectionString)
        {
            await m_backend.InitializeAsync(_connectionString);
        }

        // --- 캐시 키 생성 ---

        private static string CacheKey<T>(string _id) => $"{typeof(T).Name}:{_id}";
        private static string CacheListKey<T>() => $"{typeof(T).Name}:__all__";

        // --- 읽기: 캐시 우선 ---

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync<T>(string _id) where T : class, new()
        {
            if (m_cache.IsConnected)
            {
                string? cached = await m_cache.GetAsync(CacheKey<T>(_id));
                if (cached != null)
                    return JsonSerializer.Deserialize<T>(cached);
            }

            T? result = await m_backend.GetByIdAsync<T>(_id);
            if (result != null && m_cache.IsConnected)
                await m_cache.SetAsync(CacheKey<T>(_id), JsonSerializer.Serialize(result), m_defaultExpiry);

            return result;
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetAllAsync<T>() where T : class, new()
        {
            // GetAll은 항상 DB에서 (리스트 캐싱은 복잡도 대비 효과 낮음)
            return await m_backend.GetAllAsync<T>();
        }

        /// <inheritdoc/>
        public async Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> _predicate) where T : class, new()
        {
            return await m_backend.FindAsync(_predicate);
        }

        // --- 쓰기: DB + 캐시 동시 갱신 ---

        /// <inheritdoc/>
        public async Task AddAsync<T>(T _entity) where T : class, new()
        {
            await m_backend.AddAsync(_entity);
            await InvalidateListCache<T>();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync<T>(T _entity) where T : class, new()
        {
            await m_backend.UpdateAsync(_entity);
            await InvalidateEntityCache<T>(_entity);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync<T>(T _entity) where T : class, new()
        {
            await m_backend.DeleteAsync(_entity);
            await InvalidateEntityCache<T>(_entity);
            await InvalidateListCache<T>();
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync()
        {
            await m_backend.SaveChangesAsync();
        }

        // --- 캐시 무효화 ---

        private async Task InvalidateEntityCache<T>(T _entity) where T : class, new()
        {
            if (!m_cache.IsConnected) return;
            // Id_ 프로퍼티가 있으면 해당 키 삭제
            var idProp = typeof(T).GetProperty("Id_");
            if (idProp?.GetValue(_entity) is string id)
                await m_cache.DeleteAsync(CacheKey<T>(id));
        }

        private async Task InvalidateListCache<T>() where T : class, new()
        {
            if (!m_cache.IsConnected) return;
            await m_cache.DeleteAsync(CacheListKey<T>());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            m_backend.Dispose();
            m_cache.Dispose();
        }
    }
}
