using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateComponentMaterialVm
    {
        public Guid ComponentMaterialId { get; set; }
        public Guid? RawMaterialId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? WasteFactor { get; set; }
    }
}
