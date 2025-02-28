using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateStripeConfiguration 
    {
        [Required]
        public string? SecretKey { get; set; }
        [Required]
        public string? PublishableKey { get; set; }
        //[Required]
        public string? cancelUrl { get; set; }
        //[Required]
        public string? successUrl { get; set; }
    }
}
