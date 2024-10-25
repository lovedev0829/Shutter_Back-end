using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblTenant
{
    public Guid TenantId { get; set; }

    public Guid? ParentTenantId { get; set; }

    public string? TenantName { get; set; }

    public string? BrandingInfo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual ICollection<TblTenant> InverseParentTenant { get; set; } = new List<TblTenant>();

    public virtual TblTenant? ParentTenant { get; set; }
}
