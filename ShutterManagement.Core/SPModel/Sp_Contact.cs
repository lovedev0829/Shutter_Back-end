using ShutterManagement.Core.SPModel.SP_Responses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel
{
    public class Sp_Contact
    {

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z]+$")]
        [DefaultValue("")]
        public string ContactType { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$")]
        [DefaultValue("")]
        public string? Name { get; set; }

        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        [DefaultValue("")]
        public string? PhoneNumber { get; set; }


        [Required, MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address.")]
        [EmailAddress]
        [DefaultValue("")]
        public string Email { get; set; }

        [DefaultValue("")]
        public string? Website { get; set; }

        [DefaultValue("")]
        public string? Notes { get; set; }

        [DefaultValue("")]
        public string? Referral { get; set; }

        public bool IsBillingAddressSameAsAddress { get; set; } = false;
        public BillingAddressVm? BillingAddressVm { get; set; }
        public bool IsTaxExemption { get; set; } = false;
        public TaxExemption? TaxExemptionVm { get; set; }
    }
}
