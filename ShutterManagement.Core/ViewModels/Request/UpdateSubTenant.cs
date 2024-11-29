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
        [MinLength(8), MaxLength(12)]
        public string? Password { get; set; }
        //public string? ParentTenantId { get; set; }
    }
}
