using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class ComponentListVm
    {
        public Guid ComponentId { get; set; }
        public string? ComponentName { get; set; }
        public string? ProductType { get; set; }
        public decimal? MarkupPercentage { get; set; }
        public string? PricingExpression { get; set; }
        public string? Notes { get; set; }
        public Guid? TenantId { get; set; }
        public bool? IsActive { get; set; }

    }
}
