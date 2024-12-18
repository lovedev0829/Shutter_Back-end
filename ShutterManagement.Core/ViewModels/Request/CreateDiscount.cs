using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    using CsvHelper.Configuration.Attributes;
    using ShutterManagement.Core.Common;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class CreateDiscount
    {
       [Required(ErrorMessage = "AppliedFor type is required.")]
        public AppliedFor AppliedFor { get; set; }
        [Required(ErrorMessage = "Discount type is required.")]
        public DiscountType discountType { get; set; }
        public Guid[] CustomerId { get; set; } = Array.Empty<Guid>();

        //[Range(0, 100, ErrorMessage = "Percentage discount must be between 1 and 100.")]
        //[DefaultValue(0)]
        public decimal? PercentageDiscount { get; set; }
        //[DefaultValue(0)]
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
