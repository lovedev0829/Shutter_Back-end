using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateOrder
    {
        [Default("")]
        [Required]
        public Guid? EstimateId { get; set;}
        [Default("")]
        [Required]
        public Guid? JobId { get; set; }

        [Default("")]
        [Required]
        public Guid? ContactId { get; set; }

        [Default("")]
        //[Required]
        public Guid? taxRateId { get; set; } 
        [Default("")]
        [Required]
        public string? EstimateName { get; set; }

        [Default("")]
        [Required]
        public string? EstimateCode { get; set; }
        [Default("")]
        [Required]
        public Guid[]? WindowProductId { get; set; } = new Guid[] { };
        [Default("")]
        public string? TermsCondition { get; set; }
        [Default("")]
        public string? Notes { get; set; }
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

        [Default("")]
        [Required]
        public string? Status { get; set; }
    }
}
