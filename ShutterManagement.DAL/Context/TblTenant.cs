using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblTenant
{
    public Guid TenantId { get; set; }

    public Guid? ParentTenantId { get; set; }

    public string? TenantName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public string? Code { get; set; }

    public string? Url { get; set; }

    public Guid? BrandingInfo { get; set; }

    public virtual TblTenantBrandingInfo? BrandingInfoNavigation { get; set; }

    public virtual ICollection<TblTenant> InverseParentTenant { get; set; } = new List<TblTenant>();

    public virtual TblTenant? ParentTenant { get; set; }

    public virtual ICollection<TblContact> TblContacts { get; set; } = new List<TblContact>();

    public virtual ICollection<TblRole> TblRoles { get; set; } = new List<TblRole>();

    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>();
}
