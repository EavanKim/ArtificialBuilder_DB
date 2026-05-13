using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ArtificialBuilder_EDP.Core;
using EDPFW;

namespace ArtificialBuilder
{
    /// <summary>App/Persona/Circuit/Logic/ResponseUi 5개 서브 DB를 묶는 루트 DB ([[app-logic-separation]] 5 도메인).
    /// canon-conformance phase-6 d (2026-05-13) — EDP_Db_Engine 직접 후손 폐기 → AB_Object 매개 + composition.
    /// 도메인 함수 = m_engine (EDP_Db_Engine) 위임 facade pass-through. caller API 식별자 보존. instance-model §4 정합.
    /// </summary>
    public class AB_DB : AB_Object, IAsyncDisposable
    {
        private readonly EDP_Db_Engine m_engine;

        public AB_DB() : base("DB")
        {
            m_engine = new EDP_Db_Engine();
        }

        /// <summary>EDPFW 내부 통합 경계 — EDP_Shard_Manager 등 EDPFW API 가 EDP_Db_Engine 타입을 요구할 때 implicit 변환.
        /// 외부 caller 는 AB_DB 의 facade 메서드 직접 사용 권장 (본 implicit 는 EDPFW boundary 전용).</summary>
        public static implicit operator EDP_Db_Engine(AB_DB _db) => _db.m_engine;

        // --- 초기화 ---

        /// <summary>5 도메인 DB 초기화 + 활성 페르소나 로드 ([[app-logic-separation]]).</summary>
        public async Task InitializeAsync()
        {
            // (canon-conformance phase-4 a, 2026-05-13) sub_DB 5 종 = DI Container 가 lifecycle 관리. holder 폐기 + lookup property.
            // 본 InitializeAsync 는 host this 매개 Initialize 호출만 책임.
            App.Initialize(this);
            Persona.Initialize(this);
            Circuit.Initialize(this);
            Logic.Initialize(this);
            ResponseUi.Initialize(this);

            // Timer Sync 제거 — 칠판 Tick에서 SyncDirtyToFile() 실행
            await Persona.LoadActiveAsync();
        }

        // --- DB 노드 (DI lookup property — canon-conformance phase-4 a) ---

        /// <summary>전역 앱 DB (모델 설정/UI 템플릿/파이프라인). 로직과 무관 ([[app-logic-separation]]).</summary>
        public AB_App_Db App => AB_Engine.GetService<AB_App_Db>();

        /// <summary>활성 페르소나 DB (별도 schema 보존 — chat 메시지 = 로직 히스토리 동의어).</summary>
        public AB_Persona_Db Persona => AB_Engine.GetService<AB_Persona_Db>();

        /// <summary>활성 Circuit DB (사용자 완성품 — 노드 그래프 + 자원).</summary>
        public AB_Circuit_Db Circuit => AB_Engine.GetService<AB_Circuit_Db>();

        /// <summary>활성 Logic DB (per-logic — 노드 정보만 / 사용 서킷+모델 키+ResponseUI 키+sub-logic+history).</summary>
        public AB_Logic_Db Logic => AB_Engine.GetService<AB_Logic_Db>();

        /// <summary>활성 Response UI DB (per-response-ui — 화면 구성 / Window+Component+Layer+Template).</summary>
        public AB_Response_Ui_Db ResponseUi => AB_Engine.GetService<AB_Response_Ui_Db>();

        // ───────────────────────────────────────────────────────────
        //  EDP_Db_Engine facade pass-through — caller API 식별자 보존
        // ───────────────────────────────────────────────────────────

        public int OpenDatabase<TContext>(string? _filePath, Func<DbContextOptions<TContext>, TContext> _contextFactory) where TContext : DbContext
            => m_engine.OpenDatabase(_filePath, _contextFactory);

        public ValueTask CloseAsync(int _handle) => m_engine.CloseAsync(_handle);

        public Task<T?> GetByIdAsync<T>(int _handle, object _id) where T : class => m_engine.GetByIdAsync<T>(_handle, _id);

        public Task<IEnumerable<T>> GetAllAsync<T>(int _handle) where T : class => m_engine.GetAllAsync<T>(_handle);

        public Task<IEnumerable<T>> FindAsync<T>(int _handle, Expression<Func<T, bool>> _predicate) where T : class
            => m_engine.FindAsync<T>(_handle, _predicate);

        public Task<List<TResult>> SelectAsync<T, TResult>(int _handle, Expression<Func<T, TResult>> _selector) where T : class
            => m_engine.SelectAsync<T, TResult>(_handle, _selector);

        public Task AddAsync<T>(int _handle, T _entity) where T : class => m_engine.AddAsync<T>(_handle, _entity);

        public Task AddRangeAsync<T>(int _handle, IEnumerable<T> _entities) where T : class => m_engine.AddRangeAsync<T>(_handle, _entities);

        public void Update<T>(int _handle, T _entity) where T : class => m_engine.Update<T>(_handle, _entity);

        public void Remove<T>(int _handle, T _entity) where T : class => m_engine.Remove<T>(_handle, _entity);

        public Task<int> SaveChangesAsync(int _handle) => m_engine.SaveChangesAsync(_handle);

        public Task<EDP_Db_Transaction> BeginTransactionAsync(int _handle) => m_engine.BeginTransactionAsync(_handle);

        public SqliteConnection GetRawConnection(int _handle) => m_engine.GetRawConnection(_handle);

        public void ExecuteRawSql(int _handle, string _sql) => m_engine.ExecuteRawSql(_handle, _sql);

        public int GetUserVersion(int _handle) => m_engine.GetUserVersion(_handle);

        public void SetUserVersion(int _handle, int _version) => m_engine.SetUserVersion(_handle, _version);

        public void SyncDirtyToFile() => m_engine.SyncDirtyToFile();

        public ValueTask DisposeAsync() => m_engine.DisposeAsync();
    }
}
