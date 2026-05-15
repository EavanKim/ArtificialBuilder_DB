using ArtificialBuilder.Common.Base;

namespace ArtificialBuilder.DB.Object
{
    // App DB 안 비밀 자격증명 entity row. AES 암호화 ciphertext + iv 보관.
    //   - App.Model.CredentialId nullable FK 매개 1:1 ref (model row 1 = credential row 1 매개 기본 매칭).
    //   - 암호화 키 운영 (KDF / OS 술록 DPAPI / Keychain) = 별도 그룹 db-credential-encryption.
    //   - 본 entity 정의 단계 = 컬럼만. AES 암호화 / 복호화 본체 = 별도 그룹 매개 wire.
    // EF Core POCO — AB_Context_DB 매개 DbSet<AB_Object_DB_App_Credential>.
    public class AB_Object_DB_App_Credential : AB_Object
    {
        public long Id { get; set; }

        // 사용자 라벨 ('OpenAI 본 계정' / 'Anthropic 회사 키' 등).
        public string RefName { get; set; } = string.Empty;

        // AES 암호화된 평문 비밀 (API key 본체).
        public byte[] Ciphertext { get; set; } = System.Array.Empty<byte>();

        // AES IV (per-row 고유).
        public byte[] Iv { get; set; } = System.Array.Empty<byte>();

        public override void ReceiveMessage(int _header_id, long _data_key) { }

        public override void Dispose() { }
    }
}
