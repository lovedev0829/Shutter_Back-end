using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class CreateDiscount
    {
        [Required(ErrorMessage = "Discount type is required.")]
        public int? Type { get; set; }

        [Required(ErrorMessage = "At least one customer ID is required.")]
        public Guid[]? CustomerId { get; set; }

        [Range(1, 100, ErrorMessage = "Percentage discount must be between 1 and 100.")]
        public decimal? PercentageDiscount { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Fixed amount must be greater than 0.")]
        public decimal? FixedAmount { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Expiry date is required.")]
        public DateTime? ExpiryDate { get; set; }

        public bool IsValid()
        {
            if (StartDate.HasValue && ExpiryDate.HasValue)
            {
                return ExpiryDate.Value > StartDate.Value;
            }
            return true;
        }
    }

}
