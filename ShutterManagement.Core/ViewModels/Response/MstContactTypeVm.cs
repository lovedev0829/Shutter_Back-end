using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class MstContactTypeVm 
    {
        public Guid MstContactId { get; set; }
        public string? MstContactName { get; set; }
        public bool? IsActive { get; set; }
    }
}
