using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblTaxRate
{
    public Guid TaxRateId { get; set; }

    public Guid? Region { get; set; }

    public decimal? Rate { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public string? TaxName { get; set; }

    public decimal? Municipality { get; set; }

    public decimal? State { get; set; }

    public decimal? City { get; set; }

    public virtual ICollection<TblEstimateWindowProduct> TblEstimateWindowProducts { get; set; } = new List<TblEstimateWindowProduct>();

    public virtual ICollection<TblManagePartnersProduct> TblManagePartnersProducts { get; set; } = new List<TblManagePartnersProduct>();

    public virtual ICollection<TblManagePartnersProductsOption> TblManagePartnersProductsOptions { get; set; } = new List<TblManagePartnersProductsOption>();

    public virtual ICollection<TblOption> TblOptions { get; set; } = new List<TblOption>();

    public virtual ICollection<TblOrderWindowProductOption> TblOrderWindowProductOptions { get; set; } = new List<TblOrderWindowProductOption>();

    public virtual ICollection<TblOrderWindowProduct> TblOrderWindowProducts { get; set; } = new List<TblOrderWindowProduct>();

    public virtual ICollection<TblProduct> TblProducts { get; set; } = new List<TblProduct>();

    public virtual TblTenant? Tenant { get; set; }
}
