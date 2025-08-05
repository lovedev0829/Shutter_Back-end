using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class Tenant
    {
        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabeticalError)]
        [DefaultValue("")]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabeticalError)]
        [DefaultValue("")]
        public string? MiddleName { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabeticalError)]
        [DefaultValue("")]
        public string? LastName { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [DefaultValue("")]
        public string? Code { get; set; }
        public string? Suffix { get; set; }
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

        [DefaultValue("")]
        public string? Logo { get; set; }

        [DefaultValue("#1d7e8b")]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = AppConstant.ColorCodeError)]
        public string? PrimaryColor { get; set; }

        [DefaultValue("")]
        //[RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = AppConstant.ColorCodeError)]
        public string? SecondaryColor { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabeticalError)]
        [DefaultValue("")]
        public string? UserFirstName { get; set; }

        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabeticalError)]
        [DefaultValue("")]
        public string? UserMiddleName { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabeticalError)]
        [DefaultValue("")]
        public string? UserLastName { get; set; }
        [Required, MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address.")]
        [EmailAddress]
        [DefaultValue("")]
        public string? UserEmail { get; set; }

        [Required]
        [DefaultValue("")]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? UserPhoneNumber { get; set; }

        [Required]
        [DefaultValue("")]
        [MaxLength(50)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabeticalError)]
        public string? CompanyName { get; set; }


        [Required]
        [MaxLength(100)]
        [DefaultValue("")]

        public string? StreetAddress { get; set; }


        [Required]
        [MaxLength(50)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabeticalError)]
        [DefaultValue("")]

        public string? City { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabeticalError)]
        [DefaultValue("")]

        public string? Country { get; set; }

        [Required]
        [MaxLength(6), MinLength(5)]
        [RegularExpression(@"^\d{5,6}$", ErrorMessage = AppConstant.ZipCodeError)]
        [DefaultValue("")]

        public string? ZipCode { get; set; }

        [DefaultValue("")]
        //[MinLength(3),MaxLength(3)]
        public string? InvoicePrefix { get; set; }

        //[DefaultValue(0)]
        //public decimal? Markup { get; set; }
    }
}
