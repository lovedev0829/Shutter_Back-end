using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    
        public class GhlCustomField
        {
            public string id { get; set; }
            public string name { get; set; }
            public string model { get; set; }
            public string fieldKey { get; set; }
            public string placeholder { get; set; }
            public string dataType { get; set; }
            //public int position { get; set; } 
            public string documentType { get; set; }
            public string parentId { get; set; }
            public string locationId { get; set; }
            public DateTime dateAdded { get; set; }
            public bool standard { get; set; }
        }

        public class GhlCustomFieldResponse
        {
        public List<GhlCustomField> customFields { get; set; }
        }


}
