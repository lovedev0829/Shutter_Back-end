using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class EstimateStatusNotificationVm
    {

        // Customer info
        public string? CustomerName { get; set; }
        public string? Email { get; set; }

        // Estimate details
        public string? EstimationCode { get; set; }
        public string? EstimationDate { get; set; }
        public string? PreparedBy { get; set; }
        public string? EstimationGrandTotal { get; set; }

        // Company info
        public string? TenantCompanyName { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; }
        public string? TenantId { get; set; }
        public string? EstimationStatus { get; set; }

        // Order Details 

        // Estimate details
        public string? OrderName { get; set; }
        public string? OrderDate { get; set; }
        public string? OrderStatus { get; set; }
        public string? OrderGrandTotal { get; set; }


    }

    
}
