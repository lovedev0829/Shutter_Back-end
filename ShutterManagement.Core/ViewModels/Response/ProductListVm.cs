using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class ProductListVm
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductType { get; set; }

        public string? ProductTypeName { get; set; }
        public string? Notes { get; set; }
        public decimal? Markup { get; set; }
        public bool? IsActive { get; set; }
        public Guid? TenantId { get; set; }
        public List<ProductComponents> ProductComponents { get; set; }
    }
    public class ProductComponents
    {
        public Guid ProductComponentId { get; set; }
        public decimal? Quantity { get; set; }
        public Guid? ComponentId { get; set; }
        public string? ComponentName { get; set; }
    }
}

