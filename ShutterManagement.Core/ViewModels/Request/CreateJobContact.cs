using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateJobContact
    {
        public Guid? ContactTypeId { get; set; }
        public Guid? Jobid { get; set; }

        public string? Contactname { get; set; }
    }
}
