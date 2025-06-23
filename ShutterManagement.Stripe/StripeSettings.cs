using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Stripe
{
    public class StripeSettings
    {
        public string SecretKey { get; set; }
        public string PublishableKey { get; set; }
        public string successUrl { get; set; }
        public string cancelUrl { get; set; }
        public string WebhookSecret { get; set; }


    }
}
