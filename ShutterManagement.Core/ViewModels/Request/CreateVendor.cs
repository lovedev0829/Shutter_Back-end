using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateVendor
    {
        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? VendorName { get; set; }
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
        [Required]
        [DefaultValue("")]
        public string? StreetAddress { get; set; }
        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? City { get; set; }
        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? State { get; set; }

        [Required]
        [MaxLength(6)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = AppConstant.ZipCodeError)]
        [DefaultValue("")]
        public string? ZipCode { get; set; }
    }
}
