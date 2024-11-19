using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel
{
     public class BillingAddressVm
    {

        [Required]
        [MaxLength(128)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string FullName { get; set; } = string.Empty;

      
        [Required]
        [DefaultValue("")]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; } = string.Empty ;

        [Required, MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid Email.")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]

        public string City { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]

        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [RegularExpression("^(0-9)")]

        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string StreetAddress { get; set; } = string.Empty;
    }
}
