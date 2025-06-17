using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateComponentMaterialVm
    {
        [DefaultValue("")]
        public Guid? ComponentId { get; set; }
        [DefaultValue("")]
        public Guid ComponentMaterialId { get; set; }
        [DefaultValue("")]
        public Guid? RawMaterialId { get; set; }
    
        [DefaultValue(0)]
        public decimal? Quantity { get; set; }
        [DefaultValue(0)]
        public decimal? WasteFactor { get; set; }

        public Guid MaterialPricingExpression { get; set; }
    }
}
