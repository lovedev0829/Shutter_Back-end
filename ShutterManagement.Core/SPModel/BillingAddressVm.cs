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
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? FirstName { get; set; }
        public string? BillingFirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? LastName { get; set; }
        public string? BillingLastName { get; set; }


        [DefaultValue("")]
        [RegularExpression(@"^\+?1?\d{11}$", ErrorMessage = "Phone number must start with 1 followed by 11 digits.")]
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

        //[Required]
        //[DefaultValue("")]
        public string? StateName { get; set; } = string.Empty;

        public Guid? StateId { get; set; } = Guid.Empty;
        public string? StateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(6), MinLength(5)]
        [RegularExpression(@"^\d{5,6}$", ErrorMessage = AppConstant.ZipCodeError)]
        [DefaultValue("")]
        public string? ZipCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
      //  [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? StreetAddress { get; set; } = string.Empty;

    }
}
