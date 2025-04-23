using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.SPModel.SP_Responses;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class ImportContact
    {
        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string ContactType { get; set; } = string.Empty;
        //public Guid? ContactId { get; set; } = Guid.Empty;

        [Required]
        [MaxLength(255)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? FirstName { get; set; }

        [Required]
        [MaxLength(255)]
        [RegularExpression("^[\\p{L} .'-]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
        public string? LastName { get; set; }

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
        [MaxLength(1000)]
        public string? Notes { get; set; }
        public string? Suffix { get; set; }

        [DefaultValue("")]
        public string? Referral { get; set; }
        public BillingAddressVm? BillingAddressVm { get; set; }
        public TaxExemption? TaxExemptionVm { get; set; }


    }
}
