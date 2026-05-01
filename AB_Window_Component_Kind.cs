namespace ArtificialBuilder_EDP.Core.UI.Window
{
    /// <summary>
    /// AB_Window 에 부착 가능한 컴포넌트 종류.
    /// Frame/Layout/Depth 는 AB_Window 생성 시 자동 부착되는 공통 컴포넌트.
    /// Message/Image2D/Layered2D/ThreeD/Back 은 윈도우 성격을 결정하는 조립 단위 — 0 개 이상 부착 가능.
    /// </summary>
    public enum AB_Window_Component_Kind
    {
        /// <summary>공통 — 테두리 · 제목 바 · 배경</summary>
        Frame,
        /// <summary>공통 — 도킹 Circuit · 비율 좌표 · 리사이즈/드래그</summary>
        Layout,
        /// <summary>공통 — Z-order · 투명도 · 가시성</summary>
        Depth,
        /// <summary>성격 — chat 스트림 구독 + 버블 렌더</summary>
        Message,
        /// <summary>성격 — 단일 2D 이미지 출력. 스택 불가, 1 컴포넌트 = 1 이미지</summary>
        Image2D,
        /// <summary>성격 — 스택 가능한 2D 이미지 레이어. N 개 부착 시 DepthInStack 순 쌓기</summary>
        Layered2D,
        /// <summary>성격 — 3D 오브젝트 + 애니메이션 재생</summary>
        ThreeD,
        /// <summary>성격 — 네비게이션 뒤로가기 버튼 (Target 키 발행)</summary>
        Back,
    }
}
