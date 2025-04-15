using ShutterManagement.Core.SPModel.SP_Responses;
using ShutterManagement.Core.SPModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class ContactsList : AdditionalProperty
    {
        public Guid? ContactId { get; set; }
        public string ContactType { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Suffix { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Notes { get; set; }
        public string? Referral { get; set; }
        public string? ContactUniqueId { get; set; }
        public Guid? PrimaryAddress { get; set; }
        public bool IsBillingAddressSameAsAddress { get; set; } = false;
        public BillingAddressVm billingAddressVm { get; set; } = new BillingAddressVm();
        public bool IsTaxExemption { get; set; } = false;
        public TaxExemption taxExemptionVm { get; set; } = new TaxExemption();
        public bool? IsActive { get; set; }
        public bool? IsArchive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ContactBreakdown
    {
        public string? Name { get; set; }
        public Guid? ContactId { get; set; }
        public string? ContactUniqueId { get; set; }
    }
}
