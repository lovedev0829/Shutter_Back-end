using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateEstimate
    {
        [Default("")]
        public Guid? JobId { get; set; }
        [Default("")]
        public string? EstimateName { get; set; }
        [Default("")]
        public Guid[]? WindowProductId { get; set; } = new Guid[] {};
        [Default("")]
        public string? TermsCondition { get; set; }
        [Default("")]
        public string? Notes { get; set; }
        [Default("")]
        public string? DiscountCoupon { get; set; }
        [Default(0.00)]
        public decimal? SubTotal { get; set; }
        [Default(0.00)]
        public decimal? Tax { get; set; }
        [Default(0.00)]
        public decimal? Discount { get; set; }
        [Default(0.00)]
        public decimal? GrandTotal { get; set; }
    }
}
