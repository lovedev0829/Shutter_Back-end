using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblVendor
{
    public Guid VendorId { get; set; }

    public string? VendorName { get; set; }

    public string? ContactInformation { get; set; }

    public bool? PreferredVendor { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public virtual ICollection<TblVendorOrder> TblVendorOrders { get; set; } = new List<TblVendorOrder>();

    public virtual TblTenant? Tenant { get; set; }
}
