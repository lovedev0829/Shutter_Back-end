using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{

    public class CreateManagePartnerProduct
    {
        public ManagerPartnerProducts[]? managerPartnerProducts { get; set; }
    }

    public class ManagerPartnerProducts
    {
        public Guid? ProductId { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? WholeSalePrice { get; set; }
        public decimal? MaxPartnerPrice { get; set; }
        public decimal? PartnerSellingPrice { get; set; }
        public string? TenantId { get; set; }
        public ManagePartnerProductOptions[]? ManagePartnerProductOptions { get; set; }
    }
    public class ManagePartnerProductOptions
    {
        public Guid? OptionId { get; set; }
        public Guid? OptionTypeId { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? WholeSalePrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? PartnerSellingPrice { get; set; }
    }
}
