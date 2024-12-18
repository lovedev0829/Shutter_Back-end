using CsvHelper.Configuration.Attributes;
using ShutterManagement.Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateDiscount
    {
        [Required]
        [DefaultValue("")]
        public Guid? DiscountId { get; set; }
        [Required]
        public string? CouponCode { get; set; }

        [Required(ErrorMessage = "Discount type is required.")]
        public DiscountType discountType { get; set; }

        //[Range(1, 100, ErrorMessage = "Percentage discount must be between 1 and 100.")]
        //[DefaultValue("")]
        public decimal? PercentageDiscount { get; set; }
        //[DefaultValue("")]
        //[Range(0.01, double.MaxValue, ErrorMessage = "Fixed amount must be greater than 0.")]
        public decimal? FixedAmount { get; set; }
        [DefaultValue("")]

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime? StartDate { get; set; }
        [DefaultValue("")]

        [Required(ErrorMessage = "Expiry date is required.")]
        public DateTime? ExpiryDate { get; set; }
    }
}
