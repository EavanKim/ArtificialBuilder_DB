using EDPFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// AB_DB 기반 IAB_Db_Backend 공통 베이스 클래스.
    /// 핸들/엔진을 바인딩하면 CRUD는 동일 경로를 따르므로 공통화한다.
    /// 서브클래스는 BackendName과 초기화 의미(파일명 vs 연결문자열)만 제공.
    /// </summary>
    public abstract class AB_Db_Backend_Base : ArtificialBuilder_EDP.Core.AB_Object, IAB_Db_Backend
    {
        /// <inheritdoc/>
        public abstract string BackendName { get; }
        /// <inheritdoc/>
        public bool IsConnected => m_handle != 0;

        /// <summary>바인딩된 AB_DB 인스턴스.</summary>
        protected AB_DB m_engine = null!;
        /// <summary>AB_DB 핸들 (0 = 미연결).</summary>
        protected int m_handle;

        /// <inheritdoc/>
        public abstract Task InitializeAsync(string _connectionString);

        /// <summary>AB_DB와 연결.</summary>
        public void Bind(AB_DB _engine, int _handle)
        {
            m_engine = _engine;
            m_handle = _handle;
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetAllAsync<T>() where T : class, new()
        {
            if (m_handle == 0) return new();
            return (await m_engine.GetAllAsync<T>(m_handle)).ToList();
        }

        /// <inheritdoc/>
        public async Task<T?> GetByIdAsync<T>(string _id) where T : class, new()
        {
            if (m_handle == 0) return null;
            return await m_engine.GetByIdAsync<T>(m_handle, _id);
        }

        /// <inheritdoc/>
        public async Task<List<T>> FindAsync<T>(Expression<Func<T, bool>> _predicate) where T : class, new()
        {
            if (m_handle == 0) return new();
            return (await m_engine.FindAsync(m_handle, _predicate)).ToList();
        }

        /// <inheritdoc/>
        public async Task AddAsync<T>(T _entity) where T : class, new()
        {
            if (m_handle != 0)
                await m_engine.AddAsync(m_handle, _entity);
        }

        /// <inheritdoc/>
        public Task UpdateAsync<T>(T _entity) where T : class, new()
        {
            if (m_handle != 0)
                m_engine.Update(m_handle, _entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task DeleteAsync<T>(T _entity) where T : class, new()
        {
            if (m_handle != 0)
                m_engine.Remove(m_handle, _entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync()
        {
            if (m_handle != 0)
                await m_engine.SaveChangesAsync(m_handle);
        }

        /// <inheritdoc/>
        public new virtual void Dispose() { }
    }
}
