using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblContact
{
    public Guid ContactId { get; set; }

    public string? ContactType { get; set; }

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public bool IsBillingAddressSameAsAddress { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public string? Referral { get; set; }

    public bool? IsTaxExemption { get; set; }

    public bool? IsArchive { get; set; }

    public string? ContactUniqueId { get; set; }

    public virtual ICollection<TblAddress> TblAddresses { get; set; } = new List<TblAddress>();

    public virtual ICollection<TblContactBillingAddress> TblContactBillingAddresses { get; set; } = new List<TblContactBillingAddress>();

    public virtual ICollection<TblContactRole> TblContactRoles { get; set; } = new List<TblContactRole>();

    public virtual ICollection<TblDiscount> TblDiscounts { get; set; } = new List<TblDiscount>();

    public virtual ICollection<TblJob> TblJobs { get; set; } = new List<TblJob>();

    public virtual ICollection<TblTaxExemption> TblTaxExemptions { get; set; } = new List<TblTaxExemption>();

    public virtual TblTenant? Tenant { get; set; }
}
