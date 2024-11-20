using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllMaterialVm
    {
        public string? MaterialName { get; set; }

        public decimal? UnitCost { get; set; }

        public string? UnitOfMeasurement { get; set; }

        public int? StockQuantity { get; set; }

        public int? ReorderLevel { get; set; }

        public Guid? VendorId { get; set; }

        public string? Notes { get; set; }

        public bool? IsActive { get; set; }

        public Guid? TenantId { get; set; }
        public string? SKU { get; set; }
    }
}
