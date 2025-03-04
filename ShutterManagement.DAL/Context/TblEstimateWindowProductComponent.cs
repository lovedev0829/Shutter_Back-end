using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblEstimateWindowProductComponent
{
    public Guid EstimateWindowProductComponentId { get; set; }

    public Guid EstimateId { get; set; }

    public Guid ComponentId { get; set; }

    public string? ComponentName { get; set; }

    public string? ProductType { get; set; }

    public decimal? MarkupPercentage { get; set; }

    public string? PricingExpression { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? ProductId { get; set; }

    public decimal? Wastefactor { get; set; }

    public string? ProductTypeName { get; set; }

    public decimal? ComponentPrice { get; set; }

    public Guid? WindowId { get; set; }

    public virtual TblEstimate Estimate { get; set; } = null!;

    public virtual TblTenant? Tenant { get; set; }
}
