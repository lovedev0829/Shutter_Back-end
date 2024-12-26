using ShutterManagement.Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreatePayments
    {

        [Required]
        public Guid InvoiceId { get; set; }

        [Required]
        public DateTime? PaymentDate { get; set; }

        //[Compare(nameof(PaymentDate), ErrorMessage = "Payment Due Date must be after or on the Payment Date.")]
        [Required]
        public DateTime? PaymentDueDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal? Amount { get; set; }

        [MaxLength(50)]
        public string? TransactionId { get; set; }

        [Required(ErrorMessage = "PaymentStatus is required.")]
        public PaymentStatus PaymentStatus { get; set; }

        [MaxLength(200)]
        public string? Notes { get; set; }
    }
}


