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
        [MinLength(8), MaxLength(12)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,12}$",
        ErrorMessage = "Password must be 8-12 chars with letter, number & special char.")]
        public string? Password { get; set; }
       
    }
}
