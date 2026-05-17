using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Circuit DB 안 circuit entity row. AB_Object_DB_Package 정합.
    // EF Core POCO — AB_Context_DB 매개 DbSet<AB_Object_DB_Circuit>.
    public class AB_Object_DB_Circuit : AB_Object
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public override void ReceiveMessage(int _header_id, long _data_key) { }
        public override void Dispose() { }
    }
}
