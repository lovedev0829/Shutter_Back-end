using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class WindowOption
{
    public Guid WindowoptionId { get; set; }

    public Guid? OptionId { get; set; }

    public Guid? WindowProductId { get; set; }

    public Guid? WindowId { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public decimal? AdditionalCost { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public virtual TblOption? Option { get; set; }

    public virtual TblTenant? Tenant { get; set; }

    public virtual TblWindow? Window { get; set; }

    public virtual TblWindowProduct? WindowProduct { get; set; }
}
