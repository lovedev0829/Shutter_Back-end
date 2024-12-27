using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblInvoicePayment
{
    public Guid InvoicePaymentId { get; set; }

    public Guid? InvoiceId { get; set; }

    public decimal? InvoiceAmount { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public virtual TblInvoice? Invoice { get; set; }
}
