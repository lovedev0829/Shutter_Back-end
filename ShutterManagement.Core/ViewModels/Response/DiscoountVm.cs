using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class DiscoountVm
    {

        public string? CouponCode { get; set; }

       // public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }

        public decimal? PercentageDiscount { get; set; }

        public decimal? FixedAmount { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? StartDate { get; set; }

    }
    
}
