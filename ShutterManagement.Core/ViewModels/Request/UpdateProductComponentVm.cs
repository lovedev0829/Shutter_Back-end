using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateProductComponentVm
    {
        public Guid ProductComponentId { get; set; }
        public  Guid? ComponentId { get; set; }
        public decimal? Quantity { get; set; }
    }
}
