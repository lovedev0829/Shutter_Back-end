using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Stripe
{
    public class PaymentRequest
    {
        public long Amount { get; set; } 
        public long Quantity { get; set; }
        public string ProductName { get; set; }
        public string CustomerEmail { get; set; }
        public string? InvoiceId { get; set; }
        public string? PaymentId { get; set; }

    }
}
