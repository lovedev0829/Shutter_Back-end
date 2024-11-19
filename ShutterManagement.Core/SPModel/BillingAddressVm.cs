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

        [MaxLength(128)]
        [RegularExpression("^[a-zA-Z]+$")]
        [DefaultValue("")]
        public string FullName { get; set; } = string.Empty;

      
        [DefaultValue("")]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; } = string.Empty ;

        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        [DefaultValue("")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]
        [DefaultValue("")]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]
        [DefaultValue("")]
        public string State { get; set; } = string.Empty;

        [MaxLength(6)]
        [RegularExpression(@"^\d{6}$")]
        [DefaultValue("")]
        public string ZipCode { get; set; } = string.Empty;

        [MaxLength(250)]
        [DefaultValue("")]
        public string StreetAddress { get; set; } = string.Empty;
    }
}
