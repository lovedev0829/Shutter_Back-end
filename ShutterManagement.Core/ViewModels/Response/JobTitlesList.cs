using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class JobTitlesList
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
