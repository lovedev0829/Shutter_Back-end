using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class EstimateList
    {
        public Guid EstimateId { get; set; }
        public Guid? JobId { get; set; }
        public string? EstimateName { get; set; }
        public string? EstimateCode { get; set; }
        public Guid[]? WindowProductId { get; set; } = new Guid[] { };
        public string? TermsCondition { get; set; }
        public string? Notes { get; set; }
        public string? DiscountCoupon { get; set; }
        public string? Status { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Discount { get; set; }
        public decimal? GrandTotal { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }

        public List<EstimateProductList> EstimateProductLists { get; set; }
    }

    public class EstimateProductList
    {
        public Guid EstimateWindowProductId { get; set; }
        public string? Label { get; set; }
        public string? ProductType { get; set; }
        public string? ProductTypeName { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }

        
        //public decimal? ArchTop { get; set; }
        //public string[] Options { get; set; }
        //public string[] price { get; set; }
    }
}
