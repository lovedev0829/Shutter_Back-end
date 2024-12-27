using ShutterManagement.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class PaymentsVM
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
        public PaymentStatus? PaymentStatus { get; set; } 
        public bool? IsActive { get; set; }
    }
    
}
