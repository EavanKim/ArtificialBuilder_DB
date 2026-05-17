using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Logic DB entity row.
    public class AB_Object_DB_Logic : AB_Object
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public override void ReceiveMessage(int _header_id, long _data_key) { }
        public override void Dispose() { }
    }
}
