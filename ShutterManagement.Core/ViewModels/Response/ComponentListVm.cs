using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class ComponentListVm
    {
        public Guid? ComponentId { get; set; }
        public string? ComponentName { get; set; }
        public string? ProductType { get; set; }
        public string? ProductTypeName { get; set; }
        public bool? IsActive { get; set; }
        public Guid? TenantId { get; set; }
        public decimal? MarkupPercentage { get; set; }
        public string? PricingExpression { get; set; }
        public string? Notes { get; set; }
        
        public List<ComponentMaterialVm>? ComponentMaterials { get; set; }

    }

    public class ComponentMaterialVm
    {
        public Guid ComponentMaterialId { get; set; }
        public Guid? RawMaterialId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? WasteFactor { get; set; }

        public string? MaterialName { get; set; }

        public bool?  IsActive { get; set; }
        public bool?  IsDelete { get; set; }
    }

  

}
