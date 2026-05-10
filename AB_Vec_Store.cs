using EDPFW;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ArtificialBuilder
{
    /// <summary>sqlite-vec 기반 임베딩 저장소 (로어/채팅/캐릭터 데이터).</summary>
    public class AB_Vec_Store
    {
        private readonly EDP_Db_Engine m_engine;
        private int m_dimensions = 0;
        private static string? g_vecExtensionPath;

        /// <summary>EDP_Db_Engine 참조로 생성.</summary>
        public AB_Vec_Store(EDP_Db_Engine _engine)
        {
            m_engine = _engine;
        }

        // --- 네이티브 확장 경로 ---

        private static string GetVecExtensionPath()
        {
            if (g_vecExtensionPath != null) return g_vecExtensionPath;

            string basePath = AppContext.BaseDirectory;

            // RID 결정
            string rid;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                rid = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "osx-arm64" : "osx-x64";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                rid = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";
            else
                rid = "win-x64";

            string ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dll" :
                         RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "dylib" : "so";

            // runtimes/{rid}/native/vec0.{ext}
            string path = Path.Combine(basePath, "runtimes", rid, "native", $"vec0.{ext}");
            if (File.Exists(path))
            {
                g_vecExtensionPath = path;
                return path;
            }

            // 빌드 출력 루트에 있을 수도 있음
            string rootPath = Path.Combine(basePath, $"vec0.{ext}");
            if (File.Exists(rootPath))
            {
                g_vecExtensionPath = rootPath;
                return rootPath;
            }

            throw new FileNotFoundException($"sqlite-vec 네이티브 확장을 찾을 수 없습니다: {path}");
        }

        private static void LoadVec(SqliteConnection _conn)
        {
            string path = GetVecExtensionPath();
            _conn.LoadExtension(path);
        }

        // --- 초기화 ---

        /// <summary>vec0 확장 로드 + 메타/벡터 테이블 생성.</summary>
        public void Initialize(int _handle, int _dimensions)
        {
            m_dimensions = _dimensions;

            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            LoadVec(conn);

            // 메타데이터 테이블
            m_engine.ExecuteRawSql(_handle,
                "CREATE TABLE IF NOT EXISTS vec_metadata (key TEXT PRIMARY KEY, value TEXT NOT NULL)");

            // 벡터 테이블 (dimensions는 정수만 허용 — SQL injection 방어)
            int safeDim = Math.Clamp(_dimensions, 1, 65536);
            m_engine.ExecuteRawSql(_handle,
                $"CREATE VIRTUAL TABLE IF NOT EXISTS vec_lore USING vec0(lore_id TEXT NOT NULL, embedding float[{safeDim}])");
            m_engine.ExecuteRawSql(_handle,
                $"CREATE VIRTUAL TABLE IF NOT EXISTS vec_chat USING vec0(session_id INTEGER NOT NULL, node_id TEXT NOT NULL, turn_index INTEGER NOT NULL, refresh_index INTEGER NOT NULL, emission_order INTEGER NOT NULL, embedding float[{safeDim}])");
            m_engine.ExecuteRawSql(_handle,
                $"CREATE VIRTUAL TABLE IF NOT EXISTS vec_cdata USING vec0(cdata_id TEXT NOT NULL, embedding float[{safeDim}])");

            // 차원 정보 저장
            SetMeta(_handle, "dimensions", _dimensions.ToString());
        }

        /// <summary>vec0 확장만 로드 (테이블 생성 없음).</summary>
        public void LoadExtension(int _handle)
        {
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            LoadVec(conn);
        }

        // --- float[] → byte[] 직렬화 ---

        private static byte[] SerializeFloat32(float[] _embedding)
        {
            byte[] bytes = new byte[_embedding.Length * 4];
            Buffer.BlockCopy(_embedding, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        // --- 로어북 벡터 ---

        /// <summary>로어 임베딩 삽입.</summary>
        public void InsertLoreEmbedding(int _handle, string _loreId, float[] _embedding)
        {
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO vec_lore(lore_id, embedding) VALUES (@id, @emb)";
            cmd.Parameters.AddWithValue("@id", _loreId);
            cmd.Parameters.AddWithValue("@emb", SerializeFloat32(_embedding));
            cmd.ExecuteNonQuery();
        }

        /// <summary>로어 임베딩 삭제.</summary>
        public void DeleteLoreEmbedding(int _handle, string _loreId)
        {
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM vec_lore WHERE lore_id = @id";
            cmd.Parameters.AddWithValue("@id", _loreId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>로어 벡터 유사도 검색.</summary>
        public List<(string LoreId, double Distance)> SearchLore(int _handle, float[] _query, int _topK = 5)
        {
            List<(string, double)> results = new();
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT lore_id, distance
                FROM vec_lore
                WHERE embedding MATCH @query
                ORDER BY distance
                LIMIT @k";
            cmd.Parameters.AddWithValue("@query", SerializeFloat32(_query));
            cmd.Parameters.AddWithValue("@k", _topK);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add((reader.GetString(0), reader.GetDouble(1)));
            }
            return results;
        }

        /// <summary>로어 임베딩 전체 삭제.</summary>
        public void ClearLoreEmbeddings(int _handle)
        {
            m_engine.ExecuteRawSql(_handle, "DELETE FROM vec_lore");
        }

        // --- 채팅 메모리 벡터 ---
        // 키: (session_id, node_id, turn_index, refresh_index, emission_order) — context_records 와 동일 키.

        /// <summary>채팅 컨텍스트 레코드 임베딩 삽입.</summary>
        public void InsertChatEmbedding(int _handle, long _sessionId, string _nodeId,
            int _turnIndex, int _refreshIndex, int _emissionOrder, float[] _embedding)
        {
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO vec_chat(session_id, node_id, turn_index, refresh_index, emission_order, embedding) VALUES (@sid, @nid, @ti, @ri, @eo, @emb)";
            cmd.Parameters.AddWithValue("@sid", _sessionId);
            cmd.Parameters.AddWithValue("@nid", _nodeId);
            cmd.Parameters.AddWithValue("@ti", _turnIndex);
            cmd.Parameters.AddWithValue("@ri", _refreshIndex);
            cmd.Parameters.AddWithValue("@eo", _emissionOrder);
            cmd.Parameters.AddWithValue("@emb", SerializeFloat32(_embedding));
            cmd.ExecuteNonQuery();
        }

        /// <summary>세션의 채팅 임베딩 일괄 삭제.</summary>
        public void DeleteChatEmbeddingsBySession(int _handle, long _sessionId)
        {
            if (!TableExists(_handle, "vec_chat")) return;
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM vec_chat WHERE session_id = @sid";
            cmd.Parameters.AddWithValue("@sid", _sessionId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>특정 컨텍스트 레코드의 채팅 임베딩 삭제.</summary>
        public void DeleteChatEmbeddingByRecord(int _handle, long _sessionId, string _nodeId,
            int _turnIndex, int _refreshIndex, int _emissionOrder)
        {
            if (!TableExists(_handle, "vec_chat")) return;
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM vec_chat WHERE session_id = @sid AND node_id = @nid AND turn_index = @ti AND refresh_index = @ri AND emission_order = @eo";
            cmd.Parameters.AddWithValue("@sid", _sessionId);
            cmd.Parameters.AddWithValue("@nid", _nodeId);
            cmd.Parameters.AddWithValue("@ti", _turnIndex);
            cmd.Parameters.AddWithValue("@ri", _refreshIndex);
            cmd.Parameters.AddWithValue("@eo", _emissionOrder);
            cmd.ExecuteNonQuery();
        }

        private bool TableExists(int _handle, string _tableName)
        {
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name=@name";
            cmd.Parameters.AddWithValue("@name", _tableName);
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>채팅 벡터 유사도 검색 (선택적 세션 제외).</summary>
        public List<(long SessionId, string NodeId, int TurnIndex, int RefreshIndex, int EmissionOrder, double Distance)> SearchChat(
            int _handle, float[] _query, int _topK = 10, long? _excludeSessionId = null)
        {
            List<(long, string, int, int, int, double)> results = new();
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();

            if (_excludeSessionId != null)
            {
                cmd.CommandText = @"
                    SELECT session_id, node_id, turn_index, refresh_index, emission_order, distance
                    FROM vec_chat
                    WHERE embedding MATCH @query AND session_id != @exclude
                    ORDER BY distance
                    LIMIT @k";
                cmd.Parameters.AddWithValue("@exclude", _excludeSessionId);
            }
            else
            {
                cmd.CommandText = @"
                    SELECT session_id, node_id, turn_index, refresh_index, emission_order, distance
                    FROM vec_chat
                    WHERE embedding MATCH @query
                    ORDER BY distance
                    LIMIT @k";
            }
            cmd.Parameters.AddWithValue("@query", SerializeFloat32(_query));
            cmd.Parameters.AddWithValue("@k", _topK);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add((reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetDouble(5)));
            }
            return results;
        }

        /// <summary>세션별 임베딩 목록 조회 (키 튜플 + 벡터 차원수)</summary>
        public List<(string NodeId, int TurnIndex, int RefreshIndex, int EmissionOrder, int Dimensions)> GetChatEmbeddingsBySession(
            int _handle, long _sessionId)
        {
            List<(string, int, int, int, int)> results = new();
            if (!TableExists(_handle, "vec_chat")) return results;
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT node_id, turn_index, refresh_index, emission_order, length(embedding)/4 FROM vec_chat WHERE session_id = @sid ORDER BY turn_index, refresh_index, emission_order";
            cmd.Parameters.AddWithValue("@sid", _sessionId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                results.Add((reader.GetString(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4)));
            return results;
        }

        /// <summary>세션별 임베딩 총 개수</summary>
        public int GetChatEmbeddingCount(int _handle, long _sessionId)
        {
            if (!TableExists(_handle, "vec_chat")) return 0;
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT count(*) FROM vec_chat WHERE session_id = @sid";
            cmd.Parameters.AddWithValue("@sid", _sessionId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // --- 동적 데이터 벡터 ---

        /// <summary>캐릭터 동적 데이터 임베딩 삽입.</summary>
        public void InsertCDataEmbedding(int _handle, string _cdataId, float[] _embedding)
        {
            if (!TableExists(_handle, "vec_cdata")) return;
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO vec_cdata(cdata_id, embedding) VALUES (@id, @emb)";
            cmd.Parameters.AddWithValue("@id", _cdataId);
            cmd.Parameters.AddWithValue("@emb", SerializeFloat32(_embedding));
            cmd.ExecuteNonQuery();
        }

        /// <summary>캐릭터 동적 데이터 임베딩 삭제.</summary>
        public void DeleteCDataEmbedding(int _handle, string _cdataId)
        {
            if (!TableExists(_handle, "vec_cdata")) return;
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM vec_cdata WHERE cdata_id = @id";
            cmd.Parameters.AddWithValue("@id", _cdataId);
            cmd.ExecuteNonQuery();
        }

        /// <summary>캐릭터 동적 데이터 벡터 검색.</summary>
        public List<(string CDataId, double Distance)> SearchCData(int _handle, float[] _query, int _topK = 10)
        {
            if (!TableExists(_handle, "vec_cdata")) return new();
            List<(string, double)> results = new();
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT cdata_id, distance
                FROM vec_cdata
                WHERE embedding MATCH @query
                ORDER BY distance
                LIMIT @k";
            cmd.Parameters.AddWithValue("@query", SerializeFloat32(_query));
            cmd.Parameters.AddWithValue("@k", _topK);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add((reader.GetString(0), reader.GetDouble(1)));
            }
            return results;
        }

        // --- 메타데이터 ---

        private void SetMeta(int _handle, string _key, string _value)
        {
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT OR REPLACE INTO vec_metadata(key, value) VALUES (@k, @v)";
            cmd.Parameters.AddWithValue("@k", _key);
            cmd.Parameters.AddWithValue("@v", _value);
            cmd.ExecuteNonQuery();
        }

        /// <summary>vec_metadata 키 조회.</summary>
        public string? GetMeta(int _handle, string _key)
        {
            SqliteConnection conn = m_engine.GetRawConnection(_handle);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT value FROM vec_metadata WHERE key = @k";
            cmd.Parameters.AddWithValue("@k", _key);
            return cmd.ExecuteScalar() as string;
        }

        /// <summary>vec 테이블 전체(lore/chat/cdata)의 행 수 합계.</summary>
        public int GetTotalRowCount(int _handle)
        {
            int total = 0;
            string[] tables = { "vec_lore", "vec_chat", "vec_cdata" };
            foreach (string t in tables)
            {
                if (!TableExists(_handle, t)) continue;
                try
                {
                    SqliteConnection conn = m_engine.GetRawConnection(_handle);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"SELECT count(*) FROM \"{t}\"";
                    total += Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch { }
            }
            return total;
        }

        /// <summary>vec 테이블 전체 데이터를 영구 삭제 (lore/chat/cdata). 메타/스키마는 유지.</summary>
        public void ClearAll(int _handle)
        {
            string[] tables = { "vec_lore", "vec_chat", "vec_cdata" };
            foreach (string t in tables)
            {
                if (!TableExists(_handle, t)) continue;
                try { m_engine.ExecuteRawSql(_handle, $"DELETE FROM \"{t}\""); }
                catch { }
            }
        }

        /// <summary>차원 메타가 설정되었는지 확인.</summary>
        public bool IsInitialized(int _handle)
        {
            try
            {
                string? dim = GetMeta(_handle, "dimensions");
                return dim != null && int.TryParse(dim, out int d) && d > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
