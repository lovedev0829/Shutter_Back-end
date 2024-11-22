using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblTenant
{
    public Guid TenantId { get; set; }

    public Guid? ParentTenantId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public string? Code { get; set; }

    public string? Url { get; set; }

    public Guid? BrandingInfo { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? MiddleName { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public virtual TblTenantBrandingInfo? BrandingInfoNavigation { get; set; }

    public virtual ICollection<TblTenant> InverseParentTenant { get; set; } = new List<TblTenant>();

    public virtual TblTenant? ParentTenant { get; set; }

    public virtual ICollection<TblAddress> TblAddresses { get; set; } = new List<TblAddress>();

    public virtual ICollection<TblComponentMaterial> TblComponentMaterials { get; set; } = new List<TblComponentMaterial>();

    public virtual ICollection<TblComponent> TblComponents { get; set; } = new List<TblComponent>();

    public virtual ICollection<TblContact> TblContacts { get; set; } = new List<TblContact>();

    public virtual ICollection<TblJob> TblJobs { get; set; } = new List<TblJob>();

    public virtual ICollection<TblOption> TblOptions { get; set; } = new List<TblOption>();

    public virtual ICollection<TblProductComponent> TblProductComponents { get; set; } = new List<TblProductComponent>();

    public virtual ICollection<TblRawMaterial> TblRawMaterials { get; set; } = new List<TblRawMaterial>();

    public virtual ICollection<TblRole> TblRoles { get; set; } = new List<TblRole>();

    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>();

    public virtual ICollection<TblVendor> TblVendors { get; set; } = new List<TblVendor>();
}
