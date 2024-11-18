using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblComponentMaterial
{
    public Guid ComponentMaterialId { get; set; }

    public Guid? ComponentId { get; set; }

    public Guid? RawMaterialId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? WasteFactor { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public virtual TblComponent? Component { get; set; }

    public virtual TblRawMaterial? RawMaterial { get; set; }

    public virtual TblTenant? Tenant { get; set; }
}
