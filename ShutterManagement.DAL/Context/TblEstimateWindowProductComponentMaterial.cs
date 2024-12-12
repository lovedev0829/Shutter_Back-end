using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblEstimateWindowProductComponentMaterial
{
    public Guid EstimateWindowProductComponentMaterialId { get; set; }

    public Guid EstimateId { get; set; }

    public Guid RawMaterialId { get; set; }

    public string? MaterialName { get; set; }

    public decimal? UnitCost { get; set; }

    public string? UnitOfMeasurement { get; set; }

    public decimal? StockQuantity { get; set; }

    public decimal? ReorderLevel { get; set; }

    public Guid? VendorId { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public string? Sku { get; set; }

    public bool? IsCutSheet { get; set; }

    public Guid? ProductType { get; set; }

    public virtual TblEstimate Estimate { get; set; } = null!;

    public virtual MstProductType? ProductTypeNavigation { get; set; }

    public virtual TblTenant? Tenant { get; set; }
}
