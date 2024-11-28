using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class JobStatusList
    {
        public Guid? JobStatusId { get; set; }
        public string? JobStatusName { get; set; }
        public bool? IsActive { get; set; }
    }
}
