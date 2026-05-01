using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 기존 EDP_Db_Engine/EF Core 기반 SQLite 백엔드.
    /// </summary>
    public class AB_Sqlite_Backend : AB_Db_Backend_Base
    {
        /// <inheritdoc/>
        public override string BackendName => "SQLite";

        private string m_dbName = "";

        /// <inheritdoc/>
        public override Task InitializeAsync(string _connectionString)
        {
            // _connectionString = DB 파일명 (예: "ArtificialBuilder.db")
            m_dbName = _connectionString;
            return Task.CompletedTask;
        }
    }
}
