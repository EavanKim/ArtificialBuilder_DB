using System;

namespace ArtificialBuilder.Models
{
    /// <summary>로직 라이브러리 list 1 행. UUID + 표시 이름 + 갱신 시각. AB_Logic_Db.GetLogicLibraryInfoAsync 응답 element.</summary>
    public class AB_Logic_Library_Item : ArtificialBuilder_EDP.Core.AB_Object
    {
        private long m_uuid_;
        public long Uuid_ { get { return m_uuid_; } set { m_uuid_ = value; } }

        private string m_name_ = "";
        public string Name_ { get { return m_name_; } set { m_name_ = value; } }

        private DateTime m_updatedAt_ = DateTime.UtcNow;
        public DateTime UpdatedAt_ { get { return m_updatedAt_; } set { m_updatedAt_ = value; } }

        /// <summary>UI 표시용 라벨. Name_ 비어있으면 "(이름 없음)" fallback. UUID 노출 X.</summary>
        public string DisplayLabel_
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(m_name_)) return m_name_;
                return "(이름 없음)";
            }
        }

        /// <summary>UI 표시용 갱신 시각 (로컬 시간, 사용자 친화적 format).</summary>
        public string UpdatedAtDisplay_
        {
            get
            {
                return m_updatedAt_.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            }
        }
    }
}
