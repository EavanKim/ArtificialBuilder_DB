namespace ArtificialBuilder.Models
{
    /// <summary>
    /// 노드의 추상 input / output port schema. 키 이름 + description (Kind / TypeCode / Note).
    /// 모델 ≠ 노드 — 노드는 *추상 인터페이스*만 가짐. 모델 설정은 서킷 단계 ([[node-categories]] / [[open-key-io]]).
    /// AB_Logic_Internal_Node_Model.InputsJson_ / OutputsJson_ 의 element.
    /// </summary>
    public class AB_Node_Port_Schema : ArtificialBuilder_EDP.Core.AB_Object
    {
        private string m_name_ = "";
        public string Name_ { get { return m_name_; } set { m_name_ = value; } }

        /// <summary>description.Kind_ — 자유 string (open key 모델, [[open-key-io]]).</summary>
        private string m_kind_ = "";
        public string Kind_ { get { return m_kind_; } set { m_kind_ = value; } }

        private int m_typeCode_;
        public int TypeCode_ { get { return m_typeCode_; } set { m_typeCode_ = value; } }

        private string? m_note_;
        public string? Note_ { get { return m_note_; } set { m_note_ = value; } }
    }
}
