using System.Collections.Generic;
using ArtificialBuilder.Common.Base;
using ArtificialBuilder.DB.Object;

namespace ArtificialBuilder.DB.Data
{
    public class AB_Data_DB_Logic_List : AB_Data<List<AB_Object_DB_Logic>>
    {
        public AB_Data_DB_Logic_List() : base(new List<AB_Object_DB_Logic>())
        {
        }
    }
}
