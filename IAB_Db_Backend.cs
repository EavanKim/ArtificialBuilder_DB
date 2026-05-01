using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// DB 백엔드 추상 인터페이스.
    /// SQLite, 외부 DB, Redis 캐시 등 교체 가능한 데이터 접근 레이어.
    /// </summary>
    public interface IAB_Db_Backend : IDisposable
    {
        /// <summary>백엔드 식별 이름.</summary>
        string BackendName { get; }
        /// <summary>연결 상태.</summary>
        bool IsConnected { get; }

        /// <summary>연결 문자열로 초기화.</summary>
        Task InitializeAsync(string _connectionString);

        // --- CRUD ---
        /// <summary>전체 조회.</summary>
        Task<List<T>> GetAllAsync<T>() where T : class, new();
        /// <summary>ID로 단건 조회.</summary>
        Task<T?> GetByIdAsync<T>(string _id) where T : class, new();
        /// <summary>술어로 조회.</summary>
        Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> _predicate) where T : class, new();
        /// <summary>엔티티 추가.</summary>
        Task AddAsync<T>(T _entity) where T : class, new();
        /// <summary>엔티티 갱신.</summary>
        Task UpdateAsync<T>(T _entity) where T : class, new();
        /// <summary>엔티티 삭제.</summary>
        Task DeleteAsync<T>(T _entity) where T : class, new();
        /// <summary>변경사항 저장.</summary>
        Task SaveChangesAsync();
    }

    /// <summary>
    /// 캐시 레이어 인터페이스 (Redis 등).
    /// </summary>
    public interface IDb_Cache : IDisposable
    {
        /// <summary>캐시 식별 이름.</summary>
        string CacheName { get; }
        /// <summary>연결 상태.</summary>
        bool IsConnected { get; }

        /// <summary>연결 문자열로 초기화.</summary>
        Task InitializeAsync(string _connectionString);

        /// <summary>키로 값 조회.</summary>
        Task<string?> GetAsync(string _key);
        /// <summary>값 저장 (선택적 만료).</summary>
        Task SetAsync(string _key, string _value, TimeSpan? _expiry = null);
        /// <summary>키 삭제.</summary>
        Task DeleteAsync(string _key);
        /// <summary>키 존재 여부.</summary>
        Task<bool> ExistsAsync(string _key);
        /// <summary>캐시 전체 비움.</summary>
        Task FlushAsync();
    }
}
