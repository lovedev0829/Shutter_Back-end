using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateProduct
    {
        public Guid ProductId  { get; set; }
        public string? ProductName { get; set; }
        public string? ProductType { get; set; }
        public string? Notes { get; set; }
        public decimal? Markup { get; set; }
        public bool? IsComponentAvailable { get; set; }

        public bool? IsTaxAvailable { get; set; } = true;
        public Guid? TaxId { get; set; }
    }
}
