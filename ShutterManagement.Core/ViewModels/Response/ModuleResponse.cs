using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class ModuleResponse
    {
        public string ModuleName { get; set; }
        public string ModuleDisplayName { get; set; }
        public List<PermissionResponse> Permissions { get; set; }
    }
}
