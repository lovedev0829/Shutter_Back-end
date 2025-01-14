using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class SettingUpdate
    {

        public string? Logo { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        public string? CompanyName { get; set; }

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
        public string? Country { get; set; }


        [Required]
        [MaxLength(6), MinLength(5)]
        [RegularExpression(@"^\d{5,6}$", ErrorMessage = AppConstant.ZipCodeError)]
        [DefaultValue("")]

        public string? ZipCode { get; set; }

    }
}
