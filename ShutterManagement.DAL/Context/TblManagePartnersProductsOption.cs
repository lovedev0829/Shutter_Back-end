using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblManagePartnersProductsOption
{
    public Guid ManageProductOptionId { get; set; }

    public Guid? ManageProductId { get; set; }

    public Guid? OptionId { get; set; }

    public Guid? OptionTypeId { get; set; }

    public bool? IsAvailable { get; set; }

    public decimal? WholeSalePrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal? PartnerSellingPrice { get; set; }

    public Guid? TenantId { get; set; }

    public string? PriceType { get; set; }

    public virtual TblManagePartnersProduct? ManageProduct { get; set; }

    public virtual TblOption? Option { get; set; }

    public virtual TblOptionsType? OptionType { get; set; }
}
