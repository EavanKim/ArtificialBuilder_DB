using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArtificialBuilder;

namespace ArtificialBuilder.Models
{
    /// <summary>
    /// Response UI Window 에 부착된 단일 Component (Response UI DB per-response-ui).
    /// 한 response_ui_windows 행에 대해 N 개 row (Frame/Layout/Depth + 출력 레이어).
    /// ComponentType_ = AB_Window_Component_Kind 카탈로그 태그 (frame/layout/depth/message/image/3d).
    /// 컴포넌트 고유 설정은 ConfigJson_ 에 직렬화.
    /// (example-mental-restructure-phase-b-residue sub 1) — string PK / FK → long.
    /// </summary>
    [Table("response_ui_components")]
    public class AB_Response_Ui_Component_Model : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_id_ = ArtificialBuilder_EDP.Core.AB_Engine.GetService<ArtificialBuilder.AB_Id_Issuer>().Issue();
        [Key]
        [Column("id")]
        public long Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>부모 윈도우 (response_windows.id) 참조.</summary>
        private long m_windowId_;
        [Required]
        [Column("window_id")]
        public long WindowId_
        {
            get { return m_windowId_; }
            set { m_windowId_ = value; }
        }

        /// <summary>컴포넌트 종류 태그 — "frame" · "layout" · "depth" · "message" · "image" · "3d".</summary>
        private string m_componentType_ = "";
        [Required]
        [Column("component_type")]
        public string ComponentType_
        {
            get { return m_componentType_; }
            set { m_componentType_ = value; }
        }

        /// <summary>같은 윈도우 내 부착 순서. 이미지 프레임 스택의 DepthInStack 도 이 값을 사용.</summary>
        private int m_sortOrder_;
        [Column("sort_order")]
        public int SortOrder_
        {
            get { return m_sortOrder_; }
            set { m_sortOrder_ = value; }
        }

        /// <summary>
        /// 사용자 정의 레이어 이름. null 이면 ComponentType_ fallback.
        /// frame / layout / depth 는 의미 없음. message / image / 3d 등 출력 레이어만 사용.
        /// 노드 UI 바인딩 드롭다운·Layers 패널 에서 표시되는 레이블 소스.
        /// </summary>
        private string? m_displayName_;
        [Column("display_name")]
        public string? DisplayName_
        {
            get { return m_displayName_; }
            set { m_displayName_ = value; }
        }

        /// <summary>컴포넌트 고유 설정 JSON. null 이면 기본값.</summary>
        private string? m_configJson_;
        [Column("config_json")]
        public string? ConfigJson_
        {
            get { return m_configJson_; }
            set { m_configJson_ = value; }
        }

        private DateTime m_createdAt_ = DateTime.UtcNow;
        [Column("created_at")]
        public DateTime CreatedAt_
        {
            get { return m_createdAt_; }
            set { m_createdAt_ = value; }
        }

        private DateTime m_updatedAt_ = DateTime.UtcNow;
        [Column("updated_at")]
        public DateTime UpdatedAt_
        {
            get { return m_updatedAt_; }
            set { m_updatedAt_ = value; }
        }

        public override void Reset()
        {
            base.Reset();
            m_id_ = 0L;
            m_windowId_ = 0L;
            m_componentType_ = "";
            m_sortOrder_ = 0;
            m_displayName_ = null;
            m_configJson_ = null;
            m_createdAt_ = default;
            m_updatedAt_ = default;
        }
    }
}
