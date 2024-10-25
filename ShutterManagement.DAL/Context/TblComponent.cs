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

    public virtual ICollection<TblComponentMaterial> TblComponentMaterials { get; set; } = new List<TblComponentMaterial>();

    public virtual ICollection<TblJobComponent> TblJobComponents { get; set; } = new List<TblJobComponent>();
}
