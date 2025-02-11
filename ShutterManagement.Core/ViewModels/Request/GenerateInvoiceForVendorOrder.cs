using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class GenerateInvoiceForVendorOrder
    {
        [Required]
        public Guid? VendorOrderId { get; set; }
        [Required]
        public Guid? VendorId { get; set; }
    }
}
