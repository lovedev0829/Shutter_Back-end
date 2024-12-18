using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblDiscount
{
    public Guid DiscountId { get; set; }

    public string? CouponCode { get; set; }

    public Guid? CustomerId { get; set; }

    public decimal? PercentageDiscount { get; set; }

    public decimal? FixedAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? Createdby { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? StartDate { get; set; }

    public virtual TblContact? Customer { get; set; }
}
