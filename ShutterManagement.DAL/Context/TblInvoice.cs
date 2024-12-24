using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblInvoice
{
    public Guid InvoiceId { get; set; }

    public Guid? JobId { get; set; }

    public string? InvoiceNumber { get; set; }

    public DateTime? IssueDate { get; set; }

    public DateTime? DueDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? PaidAmount { get; set; }

    public decimal? BalanceDue { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public string? Notes { get; set; }

    public virtual TblJob? Job { get; set; }

    public virtual ICollection<TblPayment> TblPayments { get; set; } = new List<TblPayment>();
}
