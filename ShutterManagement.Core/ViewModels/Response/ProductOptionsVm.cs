using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class ProductOptionsVm
    {
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? WholeSalePrice { get; set; }
        public decimal? MaxPartnerPrice { get; set; }
        public string? ManageProductId { get; set; }
        public decimal? PartnerSellingPrice { get; set; }
        public string? TenantId { get; set; }
        public string? MarkupType { get; set; }
        public string? PriceType { get; set; }
        public bool? IsComponentAvailable { get; set; }


        public decimal? MaxSellingPrice { get; set; }
    
        public decimal? MinSellingPrice { get; set; }
        public Guid? TaxRateId { get; set; }

        public List<ProductOptionsList> Options { get; set; }
    }

    public class ProductOptionsList
    {
        public Guid? OptionId { get; set; }
        public Guid? ProductId { get; set; }

        public string? OptionName { get; set; }
       // public Guid? TaxRateId { get; set; }



        public List<ProductOptionTypeList> optionTypes { get; set; }
    }

    public class ProductOptionTypeList
    {
        public Guid? TaxRateId { get; set; }
        public Guid? OptionTypeId { get; set; }

        public string? OptionTypeName { get; set; }
        public Guid? OptionId { get; set; }
        
        public decimal? Price { get; set; }
        public decimal? WholeSalePrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsAvailable { get; set; }
        public decimal? PartnerSellingPrice { get; set; }
        public string? PriceType { get; set; }
        public string? MarkupOptionType { get; set; }
        public decimal? MaxSellingPrice { get; set; }
        public decimal? MinSellingPrice { get; set; }
    }

    public class MSRPVm
    {
        public decimal? msrp { get; set; }
     
            public Dictionary<string, decimal> OptionWholesale { get; set; } = new Dictionary<string, decimal>();
            public Dictionary<string, decimal> OptionMSRP { get; set; } = new Dictionary<string, decimal>();
        public decimal? basecost { get; set; }
    }
}

