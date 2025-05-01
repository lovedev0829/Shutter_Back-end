using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblOption
{
    public Guid OptionId { get; set; }

    public string? OptionName { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? ProductId { get; set; }

    public bool? IsDefault { get; set; }

    public Guid? ProductTypeId { get; set; }

    public virtual MstProductType? ProductType { get; set; }

    public virtual ICollection<TblManagePartnersProductsOption> TblManagePartnersProductsOptions { get; set; } = new List<TblManagePartnersProductsOption>();

    public virtual ICollection<TblOptionsType> TblOptionsTypes { get; set; } = new List<TblOptionsType>();

    public virtual TblTenant? Tenant { get; set; }
}
