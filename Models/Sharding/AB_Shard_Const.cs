using ArtificialBuilder_EDP.Core;

namespace ArtificialBuilder.Sharding
{
    /// <summary>AB 프로젝트 샤딩 상수 — 샤드 키 / 버킷 수 / 파일명 규칙.
    /// Phase 4.4.d — POOL_PREFIX / POOL_BUCKET_COUNT / PoolShardKey 폐기.
    /// Phase 4.6 — CTX_PREFIX / CtxShardKey / CtxShardFile 폐기.
    /// 4 계층 storage ([[storage-layers]]) 가 정본.
    /// canon-conformance phase-5 h (2026-05-13) — static class 폐기 + AB_Object 후손.</summary>
    public class AB_Shard_Const : AB_Object
    {
        public AB_Shard_Const() : base("AB_Shard_Const") { }
    }
}
