using System.Collections.Generic;
using ArtificialBuilder.Models;

namespace ArtificialBuilder
{
    /// <summary>(windows-ddo-migration sub 2) REFRESH Result — 단순 Ok 신호.</summary>
    public sealed class AB_Character_Refresh_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private bool m_ok_;
        public bool Ok_ { get => m_ok_; set => m_ok_ = value; }

        public override void Reset()
        {
            base.Reset();
            m_ok_ = false;
        }
    }

    /// <summary>(windows-ddo-migration sub 2) GET_ALL Result — 캐릭터 리스트.</summary>
    public sealed class AB_Character_Get_All_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private List<AB_Character_Model> m_characters_ = new();
        public List<AB_Character_Model> Characters_ { get => m_characters_; set => m_characters_ = value; }

        public override void Reset()
        {
            base.Reset();
            m_characters_ = new();
        }
    }

    /// <summary>(windows-ddo-migration sub 2) GET_RELATIONSHIPS Result.</summary>
    public sealed class AB_Character_Get_Relationships_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private List<AB_Character_Relationship_Model> m_relationships_ = new();
        public List<AB_Character_Relationship_Model> Relationships_ { get => m_relationships_; set => m_relationships_ = value; }

        public override void Reset()
        {
            base.Reset();
            m_relationships_ = new();
        }
    }

    /// <summary>(windows-ddo-migration sub 2) GET_LOCATIONS Result.</summary>
    public sealed class AB_Character_Get_Locations_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private List<AB_Location_Model> m_locations_ = new();
        public List<AB_Location_Model> Locations_ { get => m_locations_; set => m_locations_ = value; }

        public override void Reset()
        {
            base.Reset();
            m_locations_ = new();
        }
    }

    /// <summary>(windows-ddo-migration sub 2) GET_CONNECTIONS Result.</summary>
    public sealed class AB_Character_Get_Connections_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private List<AB_Location_Connection_Model> m_connections_ = new();
        public List<AB_Location_Connection_Model> Connections_ { get => m_connections_; set => m_connections_ = value; }

        public override void Reset()
        {
            base.Reset();
            m_connections_ = new();
        }
    }
}
