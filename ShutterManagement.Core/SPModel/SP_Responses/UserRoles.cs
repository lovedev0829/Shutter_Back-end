using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel.SP_Responses
{
    public class UserRoles
    {
        public Guid? PermissionId { get; set; }
        public string? PermissionName { get; set; }
        public string? PermissionDisplayName { get; set; }
        public string? ModuleDisplayName { get; set; }
        public string? Icon { get; set; }
        public string? Url { get; set; }
    }
}
