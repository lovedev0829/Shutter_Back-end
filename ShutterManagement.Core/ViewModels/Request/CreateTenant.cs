using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateTenant : Tenant
    {
        [Required]
        [DefaultValue("")]
        [MinLength(6), MaxLength(24)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,12}$",
         ErrorMessage = "Password must be 6-12 digit with char")]
        public string? Password { get; set; }
       
    }
}
