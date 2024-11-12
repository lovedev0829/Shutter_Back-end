using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class Tenant
    {
        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string? FirstName { get; set; }

        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string? LastName { get; set; }
        public string? Code { get; set; }
        public string? URL { get; set; }


        [Required, MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address.")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }


        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? PhoneNumber { get; set; }
        public string? Logo { get; set; }

        [MaxLength(20)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string? PrimaryColor { get; set; }

        [MaxLength(20)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string? SecondaryColor { get; set; }

        [Required]
        [MinLength(8), MaxLength(12)]
        public string? Password { get; set; }
    }
}
