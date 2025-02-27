using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateStripeConfiguration 
    {
        public string? SecretKey { get; set; }
        public string? PublishableKey { get; set; }
        public string? cancelUrl { get; set; }
        public string? successUrl { get; set; }
    }
}
