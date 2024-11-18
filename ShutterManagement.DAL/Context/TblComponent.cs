using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblComponent
{
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

    public virtual ICollection<TblComponentMaterial> TblComponentMaterials { get; set; } = new List<TblComponentMaterial>();

    public virtual ICollection<TblJobComponent> TblJobComponents { get; set; } = new List<TblJobComponent>();

    public virtual TblTenant? Tenant { get; set; }
}
