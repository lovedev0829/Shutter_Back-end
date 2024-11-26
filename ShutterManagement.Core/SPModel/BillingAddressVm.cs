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
        [DefaultValue("")]
        //[Required]
        [MaxLength(100)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? FullName { get; set; } = string.Empty;

      
        [DefaultValue("")]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? PhoneNumber { get; set; } = string.Empty ;

        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        [DefaultValue("")]
        public string? Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? City { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? State { get; set; } = string.Empty;

        [Required]
        [MaxLength(6), MinLength(5)]
        [RegularExpression(@"^\d{5,6}$", ErrorMessage = AppConstant.ZipCodeError)]
        [DefaultValue("")]
        public string? ZipCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? StreetAddress { get; set; } = string.Empty;
    }
}
