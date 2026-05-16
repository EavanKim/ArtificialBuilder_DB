using ArtificialBuilder.Common.Base;
using EDPFW;

namespace ArtificialBuilder.DB.Component
{
    // 변동 축 1 — DB 파일 / 폴더 lifecycle family abstract.
    // canon § "Object / Component 다형성 조립 패턴" 정합. AB_Object_DB 안 m_file abstract field 매개 보유.
    // 콘크리트 2:
    //   _Normal   = 단일 파일 (App / Persona / Package 에셋 — 전체 로드)
    //   _Sharding = 폴더 + 파일 N (Node / Turn 샤딩 — lazy open)
    //
    // 본 abstract = 시그니처 만. 본체 = round 2 매개 콘크리트 안 채움.
    public abstract class AB_Component_DB_File : AB_Component
    {
        // Normal = 파일 경로 / Sharding = 폴더 경로 — 콘크리트 별 의미 다름.
        // EDP_Db_Engine = AB_Manager_DB 매개 단일 instance — Open_ 호출 site 매개 주입.
        public abstract void Open_(string _root_path, EDP_Db_Engine _engine);

        // 보유 handle 전수 close cascade.
        public abstract void Close_();

        // 보유 handle 전수 dirty flush — EDP_Db_Engine.SyncDirtyToFile 매개.
        public abstract void SyncDirtyToFile_();
    }
}
