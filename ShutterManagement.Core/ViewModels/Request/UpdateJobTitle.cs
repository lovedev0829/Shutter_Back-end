using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateJobTitle : CreateJobTitle
    {
        public Guid? Id { get; set; }
        
    }
}
