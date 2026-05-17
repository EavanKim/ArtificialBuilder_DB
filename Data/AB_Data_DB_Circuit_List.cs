using System.Collections.Generic;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    public class AB_Data_DB_Circuit_List : AB_Data<List<AB_Object_DB_Circuit>>
    {
        public AB_Data_DB_Circuit_List() : base(new List<AB_Object_DB_Circuit>())
        {
        }
    }
}
