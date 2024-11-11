using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class LoginDetailVM
    {
        [Required, MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid Username.")]
        [EmailAddress(ErrorMessage = "Invalid Username")]
        public string? Username { get; set; }

        [Required]
        [MinLength(8), MaxLength(12)]
        public string? Password { get; set; }

        [Required]  
        public string? CompanyCode { get; set; }
    }
}
