using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [DefaultValue("")]
        public string? FirstName { get; set; }

        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$")]
        [DefaultValue("")]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$")]
        [DefaultValue("")]
        public string? LastName { get; set; }
        [Required]
        [MaxLength(20)]
        [DefaultValue("")]
        public string? Code { get; set; }
        [Required]
        [MaxLength(100)]
        public string? URL { get; set; }


        [Required, MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address.")]
        [EmailAddress]
        [DefaultValue("")]
        public string? Email { get; set; }


        [Required]
        [DefaultValue("")]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? PhoneNumber { get; set; }
        public string? Logo { get; set; }

        [DefaultValue("")]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
        public string? PrimaryColor { get; set; }

        [DefaultValue("")]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
        public string? SecondaryColor { get; set; }

        //[Required]
        //[DefaultValue("")]
        //[MinLength(8), MaxLength(12)]
        //public string? Password { get; set; }
    }
}
