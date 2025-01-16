using System;
using System.Collections.Generic;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class InvoiceVM
    {
        public Guid InvoiceId { get; set; }
        public Guid? JobId { get; set; }
        public Guid? EstimateId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceDue { get; set; }
        public string? Status { get; set; }
        public string? ContactName { get; set; }
        public string? EstimateName { get; set; }
        public bool? IsActive { get; set; }

        public List<InvoicePaymentVM> InvoicePayment { get; set; } = new List<InvoicePaymentVM>();
    }

    public class InvoicePaymentVM
    {
        public Guid InvoicePaymentId { get; set; }

        public Guid? InvoiceId { get; set; }

        public decimal? InvoiceAmount { get; set; }

        public string? Notes { get; set; }

        public DateTime? PaymentDate { get; set; }

        public bool? IsActive { get; set; }
    }
}
