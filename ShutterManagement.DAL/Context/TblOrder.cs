using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblOrder
{
    public Guid OrderId { get; set; }

    public string Version { get; set; } = null!;

    public Guid EstimateId { get; set; }

    public Guid? JobId { get; set; }

    public string? EstimateName { get; set; }

    public string? EstimateCode { get; set; }

    public decimal? TotalPrice { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? GrandTotal { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? TenantId { get; set; }

    public string? Notes { get; set; }

    public string? TermsCondition { get; set; }

    public string? DiscountCoupon { get; set; }

    public string? OrderName { get; set; }

    public Guid? TaxId { get; set; }

    public decimal? AdditionalCost { get; set; }
}
