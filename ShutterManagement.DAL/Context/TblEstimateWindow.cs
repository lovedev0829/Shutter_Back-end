using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblEstimateWindow
{
    public Guid EstWindowId { get; set; }

    public Guid EstimateId { get; set; }
    public Guid WindowId { get; set; }

    public Guid? AddressId { get; set; }

    public string? Label { get; set; }

    public decimal? Width { get; set; }

    public decimal? Height { get; set; }

    public decimal? ArchHeight { get; set; }

    public string? Photo { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? TenantId { get; set; }

    public decimal? Quantity { get; set; }

    public Guid? WindowId { get; set; }

    public virtual TblAddress? Address { get; set; }

    public virtual TblEstimate Estimate { get; set; } = null!;

    public virtual TblTenant? Tenant { get; set; }

    public virtual TblWindow? Window { get; set; }
}
