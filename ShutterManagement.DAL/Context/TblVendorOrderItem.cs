using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblVendorOrderItem
{
    public Guid VendorOrderItemId { get; set; }

    public Guid? VendorOrderId { get; set; }

    public string? ItemDescription { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? MaterialId { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual TblRawMaterial? Material { get; set; }

    public virtual TblVendorOrder? VendorOrder { get; set; }
}
