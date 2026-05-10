using System.Collections.Generic;

namespace ArtificialBuilder
{
    /// <summary>JSON 직렬화용 윈도우 레이아웃 데이터 (비율 + 구 포맷 호환 절대좌표)</summary>
    public class Window_Layout_Data
    {
        /// <summary>템플릿 ID — response_windows.Id (long PK). 재시드 시 envelope 내 TemplateId 를 그대로 재사용해 DB Id 안정성 보장.</summary>
        private long m_templateId;
        public long TemplateId
        {
            get { return m_templateId; }
            set { m_templateId = value; }
        }
        /// <summary>윈도우 이름 — response_windows.Name. envelope ↔ circuit_def 매칭용 (DB 비어있을 때 TemplateId 재사용 키).
        /// 구 envelope 에는 없음 (null 허용). 구 envelope 복원 시 circuit_def 순서 fallback.</summary>
        private string? m_name;
        public string? Name
        {
            get { return m_name; }
            set { m_name = value; }
        }
        /// <summary>비율 X</summary>
        private double m_ratioX;
        public double RatioX
        {
            get { return m_ratioX; }
            set { m_ratioX = value; }
        }
        /// <summary>비율 Y</summary>
        private double m_ratioY;
        public double RatioY
        {
            get { return m_ratioY; }
            set { m_ratioY = value; }
        }
        /// <summary>비율 너비</summary>
        private double m_ratioW;
        public double RatioW
        {
            get { return m_ratioW; }
            set { m_ratioW = value; }
        }
        /// <summary>비율 높이</summary>
        private double m_ratioH;
        public double RatioH
        {
            get { return m_ratioH; }
            set { m_ratioH = value; }
        }
        /// <summary>구 포맷 절대 X</summary>
        private double m_x;
        public double X
        {
            get { return m_x; }
            set { m_x = value; }
        }
        /// <summary>구 포맷 절대 Y</summary>
        private double m_y;
        public double Y
        {
            get { return m_y; }
            set { m_y = value; }
        }
        /// <summary>구 포맷 절대 너비</summary>
        private double m_width;
        public double Width
        {
            get { return m_width; }
            set { m_width = value; }
        }
        /// <summary>구 포맷 절대 높이</summary>
        private double m_height;
        public double Height
        {
            get { return m_height; }
            set { m_height = value; }
        }
        /// <summary>투명도</summary>
        private double m_opacity = 1.0;
        public double Opacity
        {
            get { return m_opacity; }
            set { m_opacity = value; }
        }
        /// <summary>z-order</summary>
        private int m_zIndex;
        public int ZIndex
        {
            get { return m_zIndex; }
            set { m_zIndex = value; }
        }
        /// <summary>표시 여부</summary>
        private bool m_visible = true;
        public bool Visible
        {
            get { return m_visible; }
            set { m_visible = value; }
        }
    }

    /// <summary>윈도우 레이아웃 직렬화 컨테이너 (캔버스 크기 + 윈도우 목록)</summary>
    public class Window_Layout_Envelope
    {
        /// <summary>저장 시점 캔버스 너비</summary>
        private double m_canvasWidth;
        public double CanvasWidth
        {
            get { return m_canvasWidth; }
            set { m_canvasWidth = value; }
        }
        /// <summary>저장 시점 캔버스 높이</summary>
        private double m_canvasHeight;
        public double CanvasHeight
        {
            get { return m_canvasHeight; }
            set { m_canvasHeight = value; }
        }
        /// <summary>윈도우 목록</summary>
        private List<Window_Layout_Data> m_windows = new();
        public List<Window_Layout_Data> Windows
        {
            get { return m_windows; }
            set { m_windows = value; }
        }
    }
}
