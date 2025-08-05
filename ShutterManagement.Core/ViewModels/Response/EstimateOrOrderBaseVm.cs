using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class EstimateOrOrderBaseVm
    {
        public Guid? JobId { get; set; }
        public string? JobCode { get; set; }
        public string? CustomerName { get; set; }
        public Guid? TenantId { get; set; }
        public string? EstimateCode { get; set; }
        public string? OrderName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? PreparedBy { get; set; }
        public decimal? GrandTotal { get; set; }
        public string? Status { get; set; }

        // Order related fields
        public DateTime? OrderDate { get; set; }
        public string? OrderStatus { get; set; }
        public decimal? OrderGrandTotal { get; set; }
    }

}
