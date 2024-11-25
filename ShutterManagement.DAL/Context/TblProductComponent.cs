using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblProductComponent
{
    public Guid ProductComponentId { get; set; }

    public Guid? ComponentId { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? TenantId { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal? Quantity { get; set; }

    public virtual TblComponent? Component { get; set; }

    public virtual TblProduct? Product { get; set; }

    public virtual TblTenant? Tenant { get; set; }
}
