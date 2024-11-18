using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class RoleList
    {
        public Guid  RoleId { get; set; }
        public string?  RoleName { get; set; }
        public bool? IsActive { get; set; }
        public Guid? TenantId { get; set; }
        public List<RolePermissionList>? RolePermissionList { get; set; } = new List<RolePermissionList>();
    }
    public class RolePermissionList
    {
        public Guid? PermissionId { get; set; }
        //public string? PermissionName { get; set; }
        //public string? PermissionDisplayName { get; set; }
    }
}
