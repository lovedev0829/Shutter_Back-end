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
        public decimal? SubTotal { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Discount { get; set; }
        public decimal? GrandTotal { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
