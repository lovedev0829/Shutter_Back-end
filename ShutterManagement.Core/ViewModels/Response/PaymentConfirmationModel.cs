using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class PaymentConfirmationModel
    {
        public string CustomerEmail { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public decimal? RemainingBalance { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string TenantEmail { get; set; } = string.Empty;
        public string CompanyLogo { get; set; } = string.Empty;
    }


}
