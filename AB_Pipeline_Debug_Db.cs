using ArtificialBuilder_EDP;
using EDPFW;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 파이프라인 디버그 DB 라이프사이클 보관소. 전용 pipeline_debug.pdb 파일.
    /// CRUD 는 AB_Pipeline_Debug_Db_Proxy/AB_Pipeline_Debug_Db_Gateway 경유. 여기서는 핸들과 열기/닫기만.
    /// </summary>
    public class AB_Pipeline_Debug_Db
    {
        public const string DEBUG_DIR = "telemetry";
        public const string DEFAULT_FILE = "pipeline_debug.pdb";

        private EDP_Db_Engine m_engine = null!;

        /// <summary>현재 열린 DB 핸들 (0=미열림).</summary>
        private int m_handle;
        public int Handle
        {
            get { return m_handle; }
            private set { m_handle = value; }
        }

        /// <summary>EDP_Db_Engine 참조 저장. 실제 파일 열기는 OpenAsync 에서 수행.</summary>
        public void Initialize(EDP_Db_Engine _engine) { m_engine = _engine; }

        /// <summary>기본 경로(<see cref="DEBUG_DIR"/>/<see cref="DEFAULT_FILE"/>)로 DB 열기.</summary>
        public Task OpenDefaultAsync()
        {
            return OpenAsync(Path.Combine(DEBUG_DIR, DEFAULT_FILE));
        }

        /// <summary>지정 경로로 DB 열기. 디렉터리는 자동 생성.</summary>
        public async Task OpenAsync(string _filePath)
        {
            if (Handle != 0) { await m_engine.CloseAsync(Handle); Handle = 0; }
            string? dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            static AB_Pipeline_Debug_Db_Context CreateContext(DbContextOptions<AB_Pipeline_Debug_Db_Context> _options)
                => new AB_Pipeline_Debug_Db_Context(_options);

            Handle = m_engine.OpenDatabase<AB_Pipeline_Debug_Db_Context>(_filePath, CreateContext);
            ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder_EDP.AB_Log>().Debug("PipelineDebug", $"DB 열기: {_filePath}");
        }

        /// <summary>DB 닫기.</summary>
        public async Task CloseAsync()
        {
            if (Handle != 0) { await m_engine.CloseAsync(Handle); Handle = 0; }
        }
    }
}
