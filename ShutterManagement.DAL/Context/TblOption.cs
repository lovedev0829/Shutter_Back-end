using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblOption
{
    public Guid OptionId { get; set; }

    public string? OptionName { get; set; }

    public string? OptionType { get; set; }

    public Guid? ProductType { get; set; }

    public string? Description { get; set; }

    public decimal? AdditionalCost { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public virtual MstProductType? ProductTypeNavigation { get; set; }

    public virtual TblTenant? Tenant { get; set; }
}
