using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class StripeConfig
    {
        public string? StripeSecretKey { get; set; }
        public string? StripePublishableKey { get; set; }
        public string? StripeWebhookSecret { get; set; }
        public string? StripeApiKey { get; set; }
    }

}
