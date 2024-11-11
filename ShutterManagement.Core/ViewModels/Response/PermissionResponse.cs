using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class PermissionResponse
    {
        public Guid PermissionId { get; set; }
        public string ModulePermission { get; set; }
    }
}
