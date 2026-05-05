using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// TODO[db-three-way-split]: sub 3 폐기 + sub 2 신설. 본 모델 → Response UI DB 의 AB_Response_Ui_Component_Model 로 이전. plan: docs/plans/doing/db-three-way-split/2-response-ui-db-schema.md
namespace ArtificialBuilder.Models
{
    /// <summary>
    /// AB_Window 에 부착된 단일 컴포넌트의 DB 저장 형태.
    /// 하나의 response_windows 행에 대해 N 개 row 가 존재 (Frame/Layout/Depth + 성격들).
    /// ComponentType_ 은 AB_Window_Component_Kind 의 카탈로그 태그 (frame/layout/depth/message/image/3d).
    /// 컴포넌트 고유 설정은 ConfigJson_ 에 직렬화.
    /// </summary>
    [Table("window_components")]
    public class AB_Window_Component_Model
    {
        private string m_id_ = Guid.NewGuid().ToString();
        [Key]
        [Column("id")]
        public string Id_
        {
            get { return m_id_; }
            set { m_id_ = value; }
        }

        /// <summary>부모 윈도우 (response_windows.id) 참조.</summary>
        private string m_windowId_ = "";
        [Required]
        [Column("window_id")]
        public string WindowId_
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
    }
}
