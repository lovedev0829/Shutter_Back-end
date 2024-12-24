using System;
using System.Collections.Generic;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class InvoiceVM
    {
        public Guid InvoiceId { get; set; }

        public Guid? JobId { get; set; }

        public string? InvoiceNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Status { get; set; }
        public bool? IsActive { get; set; }

        public List<InvoicePaymentVM> InvoicePayments { get; set; } = new List<InvoicePaymentVM>();
    }

    public class InvoicePaymentVM
    {
        public Guid InvoicePaymentId { get; set; }

        public Guid? InvoiceId { get; set; }

        public decimal? InvoiceAmount { get; set; }

        public decimal? TotalAmount { get; set; }
        public decimal? BalanceDue => (TotalAmount ?? 0) - (InvoiceAmount ?? 0);
        public string? Notes { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string? PaymentStatus { get; set; }

        public bool? IsActive { get; set; }
    }
}
