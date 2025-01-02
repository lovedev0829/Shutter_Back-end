using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateVendorOrderItem
    {
        [Required]
        public Guid? VendorOrderId { get; set; }
        [Required]
        public Guid? VendorOrderItemId { get; set; }

        [Required]
        public Guid? MaterialId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "UnitPrice must be greater than zero.")]
        public decimal? UnitPrice { get; set; }

        //[Required(ErrorMessage = "TotalPrice is required.")]
        //[Range(0.01, double.MaxValue, ErrorMessage = "TotalPrice must be greater than zero.")]
        //public decimal TotalPrice { get; set; }
        //public decimal? TotalPrice => Quantity * UnitPrice;

    }
}
