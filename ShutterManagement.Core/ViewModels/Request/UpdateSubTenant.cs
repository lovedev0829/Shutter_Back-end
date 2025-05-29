using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateSubTenant : Tenant
    {
        public string? TenantId { get; set; }
        [DefaultValue("")]
        [MinLength(6), MaxLength(24)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{6,24}$",
         ErrorMessage = "Password must be 6-12 letters and numbers")]
        public string? Password { get; set; }
        //public string? ParentTenantId { get; set; }
    }
}
