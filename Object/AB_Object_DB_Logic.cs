using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // Logic DB entity row.
    // Content = 로직 본체 — 2026-05-18 Stage A 매개 추가. JSON / script / 매개 sequence 매개 자유 형식 매개 caller 결정.
    //   본 round 매개 raw string 매개 — 후속 round 매개 schema 매개 정형화 매개 (예: 노드 시퀀스 / DSL 등).
    public class AB_Object_DB_Logic : AB_Object
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public override void ReceiveMessage(int _header_id, long _data_key) { }
        public override void Dispose() { }
    }
}
