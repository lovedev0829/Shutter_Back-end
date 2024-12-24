using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblPayment
{
    public Guid PaymentId { get; set; }

    public Guid? InvoiceId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public decimal? Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public decimal? TotalAmount { get; set; }

    public Guid? ModifiedBy { get; set; }

    public virtual TblInvoice? Invoice { get; set; }
}
