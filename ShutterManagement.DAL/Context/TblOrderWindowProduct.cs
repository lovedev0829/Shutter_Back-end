using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblOrderWindowProduct
{
    public Guid OrderId { get; set; }

    public string Version { get; set; } = null!;

    public Guid EstimateId { get; set; }

    public Guid WindowId { get; set; }

    public Guid ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductType { get; set; }

    public decimal? Width { get; set; }

    public decimal? Height { get; set; }

    public decimal? Length { get; set; }

    public decimal? Area { get; set; }

    public decimal? AdditionalWidth { get; set; }

    public decimal? ArchHeight { get; set; }

    public string? Color { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public decimal? Markup { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? WindowProductId { get; set; }

    public Guid OrderWindowProductId { get; set; }

    public decimal? Price { get; set; }

    public string? Options { get; set; }

    public string? ProductTypeName { get; set; }

    public decimal? ProductPrice { get; set; }

    public string? TaxName { get; set; }

    public decimal? Municipality { get; set; }

    public decimal? State { get; set; }

    public decimal? City { get; set; }

    public Guid? TaxId { get; set; }

    public decimal? WholeSalePrice { get; set; }

    public virtual TblTaxRate? Tax { get; set; }

    public virtual ICollection<TblOrderWindowProductOption> TblOrderWindowProductOptions { get; set; } = new List<TblOrderWindowProductOption>();
}
