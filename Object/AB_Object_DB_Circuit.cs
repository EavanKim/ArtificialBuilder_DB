using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Circuit DB 안 circuit entity row. AB_Object_DB_Package 정합.
    // Content = 서킷 본체 — 2026-05-18 Stage B 매개 추가. 노드 그래프 JSON / DSL 매개 자유 형식 매개 caller 결정.
    //   본 round 매개 raw string 매개 — 후속 round 매개 노드 그래프 매개 schema 정형화 / visual editor 매개.
    public class AB_Object_DB_Circuit : AB_Object
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public override void ReceiveMessage(int _header_id, long _data_key) { }
        public override void Dispose() { }
    }
}
