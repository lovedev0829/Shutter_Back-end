using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateEstimate
    {
        [Default("")]
        [Required]
        public Guid? JobId { get; set; }

        //[Default("")]
        //[Required]
        //public Guid? ConctactId { get; set; }

        [Default("")]

        public Guid? taxRateId { get; set; }

        [Default("")]
        [Required]
        public string? EstimateName { get; set; }
        [Default("")]
        [Required]
        public List<ProductListing> ProductsList { get; set; } = new List<ProductListing> { };
        [Default("")]
        public string? TermsCondition { get; set; }
        [Default("")]
        public string? Notes { get; set; }

        [Default("")]
        public Guid? DiscountId { get; set; }
        [Default("")]
        public string? DiscountCoupon { get; set; }
        [Default(0.00)]
        [Required]
        public decimal? SubTotal { get; set; }
        [Default(0.00)]
        public decimal? Tax { get; set; }
        [Default(0.00)]
        public decimal? Discount { get; set; }
        [Default(0.00)]
        [Required]
        public decimal? GrandTotal { get; set; }
    }

    public class ProductListing
    {
        public Guid WindowProductId { get; set; }

        public decimal ProductPrice { get; set; }
    }

    public class DimensionModel
    {
        public decimal? Quantity { get; set; }
        public decimal? BuildHeight { get; set; }
        public decimal? BuildWidth { get; set; }
        public decimal? Optionprice { get; set; }
        public bool? IsView { get; set; }
    }

    public class OptionPriceResult
    {
        public List<decimal> OptionWholesale { get; set; } = new List<decimal>();
        public List<decimal> OptionMSRP { get; set; } = new List<decimal>();
        public decimal TotalCost { get; set; } = 0.0m; 
    }


}