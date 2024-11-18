using ShutterManagement.Core.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel
{
     public class GetContactResponse
     {
        public List<UpdateContact> Contacts { get; set; }
     }
}
