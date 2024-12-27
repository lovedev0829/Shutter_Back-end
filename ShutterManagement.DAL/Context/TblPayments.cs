using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblPayments
{
    public Guid PaymentId { get; set; }

    public Guid? InvoiceId { get; set; }

    public string? Notes { get; set; }

    public DateTime? PaymentDate { get; set; }

    public DateTime? PaymentDueDate { get; set; }

    public string? PaymentMethod { get; set; }

    public decimal? Amount { get; set; }

    public string? TransactionId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual TblInvoice? Invoice { get; set; }
}
