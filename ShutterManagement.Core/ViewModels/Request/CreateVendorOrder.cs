using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.DAL.Context;

namespace ShutterManagement.Core.ViewModels.Request
{
    using ShutterManagement.Core.Common;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class CreateVendorOrder
    {
        [DefaultValue("")]
        [Required]
        public Guid? VendorId { get; set; }

        [DefaultValue("")]
        [Required]
        public Guid? JobId { get; set; }

        [Required]
        [DefaultValue("")]
        public string? OrderNumber { get; set; }

        [DefaultValue("")]
        [Required]
        public DateTime? OrderDate { get; set; }

        [DefaultValue("")]
        [Required]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [DefaultValue("")]
        [Required]
        public DateTime? ActualDeliveryDate { get; set; }

        [Required]
        public VendorOrderStatus Status { get; set; }

        [Required]
        [DefaultValue("")]
        public string? Notes { get; set; }
        //[Required]
        //public decimal? TotalAmount { get; set; }

        [MinLength(1, ErrorMessage = "At least one VendorOrderItem is required.")]
        public List<VendorOrderItem> VendorOrderItems { get; set; } = new List<VendorOrderItem>();
    }

    public class VendorOrderItem
    {
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








