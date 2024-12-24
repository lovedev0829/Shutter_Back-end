using ShutterManagement.Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateInvoice
    {

        [Required]
        [DefaultValue("")]
        public Guid JobId { get; set; }

        [Required]
        [DefaultValue("")]
        [StringLength(50, ErrorMessage = "Invoice number cannot exceed 50 characters.")]
        public string? InvoiceNumber { get; set; }

        [Required]
       
        public DateTime? IssueDate { get; set; }

        [Required]
        public DateTime? DueDate { get; set; }

        [Required]
        [DefaultValue("")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]

        public string? Status { get; set; }

        public List<InvoicePayment> InvoicePayment { get; set; } = new List<InvoicePayment>();

    }
    public class InvoicePayment
    {

        [DefaultValue("")]
        public string? Notes { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Paid amount must be a positive value.")]
        [DefaultValue(0)]
        public decimal? InvoiceAmount { get; set; }
      
        [Required]
        [DefaultValue(0)]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be a positive value.")]
        public decimal? TotalAmount { get; set; }


    }

}
