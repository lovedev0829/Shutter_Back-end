using CsvHelper.Configuration.Attributes;
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
        [Name("BillingFirstName*")]
        public string? BillingFirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? LastName { get; set; }

        [Name("BillingLastName*")]
        public string? BillingLastName { get; set; }


        [DefaultValue("")]
        [RegularExpression(@"^\+?1?\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        [Name("PhoneNumber*")]
        public string? PhoneNumber { get; set; } = string.Empty ;

        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        [DefaultValue("")]
        [Name("Email*")]
        public string? Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        [Name("City*")]
        public string? City { get; set; } = string.Empty;

        //[Required]
        //[DefaultValue("")]
        [Name("StateName*")]
        public string? StateName { get; set; } = string.Empty;

        public Guid? StateId { get; set; } = Guid.Empty;
        public string? StateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(6), MinLength(5)]
        [RegularExpression(@"^\d{5,6}$", ErrorMessage = AppConstant.ZipCodeError)]
        [DefaultValue("")]
        [Name("ZipCode*")]
        public string? ZipCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
      //  [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        [Name("StreetAddress*")]
        public string? StreetAddress { get; set; } = string.Empty;

    }
}
