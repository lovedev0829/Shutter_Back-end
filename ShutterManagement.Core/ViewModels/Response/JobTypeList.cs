using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class JobTypeList
    {
        public Guid? JobTypeId { get; set; }
        public string? JobTypeName { get; set; }
        public bool? IsActive { get; set; }
    }
}
