using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class ExportMaterialsCsv
    {

        public Guid MaterialId { get; set; }
        public string? MaterialName { get; set; }

        public decimal? UnitCost { get; set; }

        public string? UnitOfMeasurement { get; set; }

        public decimal? StockQuantity { get; set; }

        public decimal? ReorderLevel { get; set; }

        public Guid? VendorId { get; set; }

        public string? VendorName { get; set; }
        public string? VendorCode { get; set; }
        public string? ProductTypeName { get; set; }


        public string? Notes { get; set; }
        public Guid? ProductType { get; set; }

        public bool? IsActive { get; set; }

        public Guid? TenantId { get; set; }
        public string? SKU { get; set; }

        public bool? IsCutSheet { get; set; }

    }
}
