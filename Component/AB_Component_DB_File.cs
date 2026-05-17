using System;
using System.Threading.Tasks;
using ArtificialBuilder.Common.Base;
using EDPFW;

namespace ArtificialBuilder.DB.Component
{
    // 변동 축 1 — DB 파일 / 폴더 lifecycle family abstract.
    // canon § "Object / Component 다형성 조립 패턴" 정합. AB_Object_DB 안 m_file abstract field 매개 보유.
    // 콘크리트 2:
    //   _Normal   = 단일 파일 (App / Persona / Package 에셋 — 전체 로드)
    //   _Sharding = 폴더 + 파일 N (Result 노드별 키 샤딩 / Turn 시계열 샤딩 — lazy open)
    //
    // D6=A — opener delegate 매개 TContext 결정 위치 콘크리트 Object 이양. File 추상 = non-generic 유지.
    public abstract class AB_Component_DB_File : AB_Component
    {
        // Normal = 파일 경로 / Sharding = 폴더 경로 — 콘크리트 별 의미 다름.
        // EDP_Db_Engine = AB_Manager_DB 매개 단일 instance — Open_ 호출 site 매개 주입.
        // _opener = `(engine, file_path) => engine.OpenDatabase<TContext>(file_path, factory)` opaque delegate. 콘크리트 Object 가 m_opener 매개 주입.
        public abstract void Open_(string _root_path, EDP_Db_Engine _engine, Func<EDP_Db_Engine, string, int> _opener);

        // 보유 handle 전수 close cascade.
        public abstract void Close_();

        // 트랜잭션 발급 — caller (Object / Manager) 매개 batch 제어 (D5=b). Normal = 단일 handle. Sharding = NotSupported (shard_key overload 사용).
        public abstract Task<EDP_Db_Transaction> BeginTransactionAsync_();

        // 샤딩 트랜잭션 발급 — shard_key 매개 handle 선택 후 BeginTransactionAsync. Sharding family 전용. Normal = NotSupported.
        // FK 최저화 룰 매개 무결성 = 어플리케이션 책임 — 본 메서드 = 단일 shard 파일 안 batch 보장 만.
        public abstract Task<EDP_Db_Transaction> BeginTransactionAsync_(long _shard_key);

        // 보유 handle 전수 dirty flush — EDP_Db_Engine.SyncDirtyToFile 매개 (engine 자체가 전 entry iterate).
        public abstract void SyncDirtyToFile_();
    }
}
