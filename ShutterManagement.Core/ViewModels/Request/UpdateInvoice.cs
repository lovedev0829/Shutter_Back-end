using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateInvoice :CreateInvoice
    {

        [Required]
        public Guid InvoiceId { get; set; }

        [Required]
        public string? InvoiceNumber { get; set; }
    }

}

