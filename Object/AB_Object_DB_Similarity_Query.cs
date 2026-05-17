using System.Collections.Generic;
using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // 13r4b 매개 FIND_SIMILAR 매개 입력 묶음. caller 매개 QueryVector (float[]) + TopK 채움 + 칠판 Register → data_key.
    // HandleResultDb FIND_SIMILAR case 매개 sqlite-vec vec_distance_cosine 매개 단일 shard 안 active row (IsActive=1) 매개 ORDER BY + LIMIT TopK.
    // 결과 = TargetDataId 매개 AB_Data_DB_Node_List slot 매개 set (row N).
    //
    // dim = caller 책임 (노드 매개 model 매개 결정) — DB 매개 검증 X. mismatch 매개 sqlite-vec 매개 throw.
    public class AB_Object_DB_Similarity_Query : AB_Object
    {
        // query embedding vector. caller 매개 채움 — AB_Object_AI_Embedding 매개 InputText → Embedding 매개 후 본 vector 매개 set.
        public float[]? QueryVector { get; set; }

        // top_k — 반환 row 수 상한.
        public int TopK { get; set; }

        public AB_Object_DB_Similarity_Query()
        {
            QueryVector = null;
            TopK = 0;
        }

        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose()
        {
            QueryVector = null;
            TopK = 0;
        }
    }
}
