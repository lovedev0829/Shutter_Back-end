using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblVendor
{
    public Guid VendorId { get; set; }

    public string? VendorName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? StreetAddress { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string? VendorCode { get; set; }

    public virtual ICollection<TblRawMaterial> TblRawMaterials { get; set; } = new List<TblRawMaterial>();

    public virtual ICollection<TblVendorOrder> TblVendorOrders { get; set; } = new List<TblVendorOrder>();

    public virtual TblTenant? Tenant { get; set; }
}
