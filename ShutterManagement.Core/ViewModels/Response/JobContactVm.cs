using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class JobContactVm
    {
        public Guid? JobContactId { get; set; }

        public Guid? JobId { get; set; }

        public Guid? ContactId { get; set; }
        public Guid? ContactTypeId { get; set; }
        public string? Contactname { get; set; }
        public string? ContactTypeName { get; set; }
        public bool? IsActive { get; set; }
    }
}
