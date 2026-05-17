using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Model DB entity row.
    // ModelType = chat / image / audio / video / embedding (UI 매개 LLM/VLM/ALM/VGM/EMB 매핑).
    public class AB_Object_DB_Model : AB_Object
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ModelType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public override void ReceiveMessage(int _header_id, long _data_key) { }
        public override void Dispose() { }
    }
}
