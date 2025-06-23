using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class UsersList
    {
        public string? FirstName { get; set; }
         public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? GhlUserName { get; set; }
        public string? GhlUserId { get; set; }
        public string? Suffix { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Guid? RoleId { get; set; }
        public string? RoleName { get; set; }
        public Guid? JobTitle { get; set; }
        public string? JobTitleName { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public Guid? TenantId { get; set; }
        public bool? IsActive { get; set; }
    }
}
