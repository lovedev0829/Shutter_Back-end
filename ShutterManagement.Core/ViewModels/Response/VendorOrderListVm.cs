using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class VendorOrderListVm
    {
        public Guid? VendorOrderId { get; set; }
        public Guid? VendorId { get; set; }
        public Guid? JobId { get; set; }
        public string? OrderNumber { get; set; }
        public string? Vendorname { get; set; }
        public string? JobName { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? Notes { get; set; }

        public decimal? TotalAmount { get; set; }
        public int? Status { get; set; }
        public string? VendorOrderStatus { get; set; }
        public bool? IsActive { get; set; }
        public List<VendorOrderItem> VendorOrderItems { get; set; } = new List<VendorOrderItem>();

    }
    public class VendorOrderItem
    {
        public Guid? MaterialId { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        //public decimal? TotalPrice { get; set; }
        public bool? IsActive { get; set; }
        //public decimal TotalPrice => Quantity * UnitPrice;
    }
}
