using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel.SP_Responses
{
    public class LoginDetails
    {
        public Guid UserId { get; set; }

        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? suffix { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Code { get; set; }
        public string? Logo { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? CompanyName { get; set; }
        public Guid? TenantId { get; set; }
        public string? ParentTenantId { get; set; }

    }
}
