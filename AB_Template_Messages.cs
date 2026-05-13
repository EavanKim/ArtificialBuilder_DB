using System.Collections.Generic;
using ArtificialBuilder.Models;

namespace ArtificialBuilder
{
    // ----- Payload -----

    public sealed class AB_Template_Add_Circuit_Payload : ArtificialBuilder_EDP.Core.AB_Object
    {
        private AB_Response_Ui_Template_Model? m_template_;
        public AB_Response_Ui_Template_Model? Template_ { get => m_template_; set => m_template_ = value; }
    }

    public sealed class AB_Template_Delete_Circuit_Payload : ArtificialBuilder_EDP.Core.AB_Object
    {
        private AB_Response_Ui_Template_Model? m_template_;
        public AB_Response_Ui_Template_Model? Template_ { get => m_template_; set => m_template_ = value; }
    }

    public sealed class AB_Template_Save_Circuit_Payload : ArtificialBuilder_EDP.Core.AB_Object
    {
        private AB_Response_Ui_Template_Model? m_template_;
        public AB_Response_Ui_Template_Model? Template_ { get => m_template_; set => m_template_ = value; }
    }

    public sealed class AB_Template_Set_Active_Circuit_Payload : ArtificialBuilder_EDP.Core.AB_Object
    {
        private string m_templateId_ = "";
        public string TemplateId_ { get => m_templateId_; set => m_templateId_ = value; }
    }

    public sealed class AB_Template_Import_From_Global_Payload : ArtificialBuilder_EDP.Core.AB_Object
    {
        private string m_globalTemplateId_ = "";
        public string GlobalTemplateId_ { get => m_globalTemplateId_; set => m_globalTemplateId_ = value; }
        private int m_sortOrder_;
        public int SortOrder_ { get => m_sortOrder_; set => m_sortOrder_ = value; }
    }

    // ----- Result -----

    public sealed class AB_Template_Get_All_Circuit_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private List<AB_Response_Ui_Template_Model> m_templates_ = new();
        public List<AB_Response_Ui_Template_Model> Templates_ { get => m_templates_; set => m_templates_ = value; }
    }

    public sealed class AB_Template_Get_All_Global_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private List<AB_Ui_Template_Model> m_templates_ = new();
        public List<AB_Ui_Template_Model> Templates_ { get => m_templates_; set => m_templates_ = value; }
    }

    public sealed class AB_Template_Add_Circuit_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private bool m_ok_;
        public bool Ok_ { get => m_ok_; set => m_ok_ = value; }
    }

    public sealed class AB_Template_Delete_Circuit_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private bool m_ok_;
        public bool Ok_ { get => m_ok_; set => m_ok_ = value; }
    }

    public sealed class AB_Template_Save_Circuit_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private bool m_ok_;
        public bool Ok_ { get => m_ok_; set => m_ok_ = value; }
    }

    public sealed class AB_Template_Set_Active_Circuit_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private bool m_ok_;
        public bool Ok_ { get => m_ok_; set => m_ok_ = value; }
    }

    public sealed class AB_Template_Import_From_Global_Result : ArtificialBuilder_EDP.Core.AB_Object
    {
        private bool m_ok_;
        public bool Ok_ { get => m_ok_; set => m_ok_ = value; }
    }
}
