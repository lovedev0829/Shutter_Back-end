using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateProduct
    {
        public string? ProductName { get; set; }
        public string? ProductType { get; set; }
        public string? Notes { get; set; }
        public decimal? Markup { get; set; }
        public List<ProductComponentVm> ProductComponents { get; set; }
    }
    public class ProductComponentVm
    {
        public decimal? Quantity { get; set; }
        public Guid? ComponentId { get; set; }
    }
}
