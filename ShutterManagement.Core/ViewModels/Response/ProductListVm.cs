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
        public bool? IsComponentAvailable { get; set; }

        public bool? IsTaxAvailable { get; set; }
        public Guid? TaxId { get; set; }

        public string? TaxName { get; set; }

        public Guid? TenantId { get; set; }
        public List<ProductComponentVmm> ProductComponents { get; set; } = new List<ProductComponentVmm>();
    }
    public class ProductComponentVmm
    {
      //  public Guid? ProductId { get; set; }
        public Guid ProductComponentId { get; set; }
        public string? PricingExpression { get; set; }
        public string? PricingExpressionName { get; set; }
        public string[]? PricingExpressionFormula { get; set; }
        public decimal? Quantity { get; set; }
        public Guid? ComponentId { get; set; }
        public string? ComponentName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }
    }
}

