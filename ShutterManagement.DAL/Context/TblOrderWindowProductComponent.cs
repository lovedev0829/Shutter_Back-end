using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblOrderWindowProductComponent
{
    public Guid OrderId { get; set; }

    public string Version { get; set; } = null!;

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

    public Guid OrderWindowProductComponentId { get; set; }
}
