using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Stripe
{
    public class StripePaymentVm
    {
        public Guid Id { get; set; }
        public string? PaymentId { get; set; }
        public decimal? PaymentAmount { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentStatus { get; set; }
        public Guid? TenantId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? WebhookResponse { get; set; }
        public string? StripeEventType { get; set; }
        public string? StripeTranjectionId { get; set; }
        public Guid? InvoiceId { get; set; }
    }

}
