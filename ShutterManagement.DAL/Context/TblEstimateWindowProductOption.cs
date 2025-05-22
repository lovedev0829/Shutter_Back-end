using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblEstimateWindowProductOption
{
    public Guid Id { get; set; }

    public Guid? WindowProductId { get; set; }

    public Guid? WindowOptionId { get; set; }

    public Guid? EstimateWindowProductId { get; set; }

    public Guid? TaxId { get; set; }

    public decimal? CityTax { get; set; }

    public decimal? StateTax { get; set; }

    public decimal? MunicipilityTax { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public string? OptionName { get; set; }

    public string? TaxName { get; set; }

    public string? OptionTypeName { get; set; }

    public decimal? OptionPrice { get; set; }

    public Guid? OptionTypeid { get; set; }

    public Guid? Optionid { get; set; }

    public virtual TblEstimateWindowProduct? EstimateWindowProduct { get; set; }

    public virtual TblTaxRate? Tax { get; set; }

    public virtual WindowOption? WindowOption { get; set; }

    public virtual TblWindowProduct? WindowProduct { get; set; }
}
