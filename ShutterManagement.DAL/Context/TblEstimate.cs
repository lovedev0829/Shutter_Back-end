using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblEstimate
{
    public Guid EstimateId { get; set; }

    public Guid? JobId { get; set; }

    public string? EstimateName { get; set; }
    public string? EstimateNumber { get; set; }

    public decimal? TotalPrice { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? GrandTotal { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual TblJob? Job { get; set; }
}
