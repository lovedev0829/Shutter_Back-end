using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllMaterialVm
    {
        public Guid MaterialId { get; set; }
        public string? MaterialName { get; set; }

        public decimal? UnitCost { get; set; }
        public decimal? NonRatedUnitCost { get; set; }

        public string? UnitOfMeasurement { get; set; }

        public decimal? StockQuantity { get; set; }

        public decimal? ReorderLevel { get; set; }

        public Guid? VendorId { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }

        public string? Notes { get; set; }
        public Guid? ProductType { get; set; }
        public string? ProductTypeName { get; set; }
        public decimal? Lenght { get; set; }
        public decimal? Width { get; set; }
        public bool? IsActive { get; set; }

        public Guid? TenantId { get; set; }
        public string? SKU { get; set; }

        public bool? IsCutSheet { get; set; }
    }
}
