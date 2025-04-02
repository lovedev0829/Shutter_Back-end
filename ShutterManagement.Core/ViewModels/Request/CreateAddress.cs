using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
     public class CreateAddress
    {

        public string? ContactId { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
       
        public string? Label { get; set; }

        [Required]
        [MaxLength(100)]
        [DefaultValue("")]
       
        public string? StreetAddress { get; set; }


        [Required]
        [MaxLength(50)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
      
        public string? City { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
      
        public Guid? State { get; set; }

        [Required]
        [MaxLength(6),MinLength(5)]
        [RegularExpression(@"^\d{5,6}$", ErrorMessage = AppConstant.ZipCodeError)]
        [DefaultValue("")]
     
        public string? ZipCode { get; set; }

        [DefaultValue("")]
        [MaxLength(1000)]
        public string?  Notes { get; set; }

        [MaxLength(20)]
        [DefaultValue("")]
        public string? AccessCode { get; set; }

       // [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? FirstName { get; set; }

        //[Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? LastName { get; set; }

        //[Required,
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address.")]
        [EmailAddress]
        [DefaultValue("")]
        public string? Email { get; set; }

        //[Required]
        [DefaultValue("")]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string Phone { get; set; }





    }
}
