using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateTenant : Tenant
    {
        public string? TenantId { get; set; }
        [DefaultValue("")]
        [MinLength(6), MaxLength(24)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,12}$",
         ErrorMessage = "Password must be 6-12 digit with char")]
        public string? Password { get; set; }

    }
}
