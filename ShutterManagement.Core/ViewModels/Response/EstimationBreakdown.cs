using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class EstimationBreakdown
    {
        public Guid? ProductTypeId { get; set; }
        public string? ProductTypeName { get; set; }

        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? OptionName { get; set; }
        public string? OptionTypeName { get; set; }

        public decimal? Quantity { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public List<ProductComponent> productComponents { get; set; }

    }
    public class ProductComponent
    {
        public decimal? Price { get; set; }
        public string? ComponentName { get; set; }

    }

}
