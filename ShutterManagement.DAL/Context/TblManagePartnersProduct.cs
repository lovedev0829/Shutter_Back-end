using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblManagePartnersProduct
{
    public Guid ManageProductId { get; set; }

    public Guid? ProductId { get; set; }

    public bool? IsAvailable { get; set; }

    public decimal? WholeSalePrice { get; set; }

    public decimal? MaxPartnerPrice { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal? PartnerSellingPrice { get; set; }

    public Guid? TenantId { get; set; }

    public decimal? MarkupPer { get; set; }

    public string? MarkupType { get; set; }

    public string? PriceType { get; set; }

    public virtual TblProduct? Product { get; set; }

    public virtual ICollection<TblManagePartnersProductsOption> TblManagePartnersProductsOptions { get; set; } = new List<TblManagePartnersProductsOption>();

    public virtual TblTenant? Tenant { get; set; }
}
