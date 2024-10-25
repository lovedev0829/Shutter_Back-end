using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblVendorOrder
{
    public Guid VendorOrderId { get; set; }

    public Guid? VendorId { get; set; }

    public Guid? JobId { get; set; }

    public string? OrderNumber { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    public string? Status { get; set; }

    public decimal? TotalCost { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual TblJob? Job { get; set; }

    public virtual ICollection<TblVendorOrderItem> TblVendorOrderItems { get; set; } = new List<TblVendorOrderItem>();

    public virtual TblVendor? Vendor { get; set; }
}
