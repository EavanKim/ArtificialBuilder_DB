using System;

namespace ArtificialBuilder.Models
{
    /// <summary>로직 라이브러리 list 1 행. UUID + 표시 이름 + 갱신 시각. AB_Logic_Db.GetLogicLibraryInfoAsync 응답 element.</summary>
    public class AB_Logic_Library_Item
    {
        private string m_uuid_ = "";
        public string Uuid_ { get { return m_uuid_; } set { m_uuid_ = value; } }

        private string m_name_ = "";
        public string Name_ { get { return m_name_; } set { m_name_ = value; } }

        private DateTime m_updatedAt_ = DateTime.UtcNow;
        public DateTime UpdatedAt_ { get { return m_updatedAt_; } set { m_updatedAt_ = value; } }
    }
}
