using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ShutterManagement.DAL.Context;

public partial class TblOptionsType
{
    public Guid OptionTypeId { get; set; }

    public Guid? OptionId { get; set; }

    public string? OptionType { get; set; }

    [DefaultValue("")]
    public decimal? AdditionalCost { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public bool? IsDefault { get; set; }

    public int? SortOrder { get; set; }

    public bool? RequirePriceAtEstimatePreview { get; set; }

    public bool hidePreview{ get; set; }

    public virtual TblOption? Option { get; set; }

    public virtual ICollection<TblManagePartnersProductsOption> TblManagePartnersProductsOptions { get; set; } = new List<TblManagePartnersProductsOption>();

    public virtual ICollection<WindowOption> WindowOptions { get; set; } = new List<WindowOption>();
}
