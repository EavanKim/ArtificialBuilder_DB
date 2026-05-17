using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    // AB_Data<AB_Object_DB_Similarity_Query?> 콘크리트 — 13r4b 매개 FIND_SIMILAR 입력 슬롯.
    // caller 매개 new + Register → data_key 발급 → AB_Manager_DB.EnqueueRequest(FIND_SIMILAR, data_key, output_node_list_id, shard_key).
    public class AB_Data_DB_Similarity_Query : AB_Data<AB_Object_DB_Similarity_Query?>
    {
        public AB_Data_DB_Similarity_Query() : base(null)
        {
        }
    }
}
