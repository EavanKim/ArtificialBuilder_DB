using ArtificialBuilder_EDP;
using EDPFW;
using System;

namespace ArtificialBuilder
{
    /// <summary>
    /// 전역 앱 DB (ArtificialBuilder.db) 라이프사이클 보관소.
    /// CRUD 는 AB_App_Db_Proxy/AB_App_Db_Gateway 경유. 여기서는 핸들과 임베딩 직렬화 헬퍼만 유지.
    /// </summary>
    public class AB_App_Db
    {
        // --- 초기화 ---

        /// <summary>AB_DB로 ArtificialBuilder.db 열기.</summary>
        public void Initialize(AB_DB _engine)
        {
            m_engine = _engine;
            Handle = _engine.OpenDatabase<AB_App_Db_Context>(
                "ArtificialBuilder.db",
                AppDbContextFactory);
            ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Info("AppDb", "앱 DB 초기화 완료.");
        }

        private static AB_App_Db_Context AppDbContextFactory(Microsoft.EntityFrameworkCore.DbContextOptions<AB_App_Db_Context> _options)
        {
            return new AB_App_Db_Context(_options);
        }

        // --- float[] 임베딩 직렬화 (세션 저장 이미지에서 재사용) ---

        /// <summary>float[] → byte[] 직렬화 (Little-Endian).</summary>
        public static byte[] SerializeEmbedding(float[] _vec)
        {
            byte[] bytes = new byte[_vec.Length * 4];
            Buffer.BlockCopy(_vec, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        /// <summary>byte[] → float[] 역직렬화.</summary>
        public static float[]? DeserializeEmbedding(byte[] _bytes, int _dimensions)
        {
            if (_bytes == null || _dimensions <= 0 || _bytes.Length < _dimensions * 4) return null;
            float[] vec = new float[_dimensions];
            Buffer.BlockCopy(_bytes, 0, vec, 0, _dimensions * 4);
            return vec;
        }

        // --- 프로퍼티 ---

        /// <summary>AB_DB 내부 핸들 (0=미초기화).</summary>
        private int m_handle;
        public int Handle
        {
            get { return m_handle; }
            private set { m_handle = value; }
        }

        // --- 멤버 변수 ---

        private AB_DB m_engine = null!;
    }
}
