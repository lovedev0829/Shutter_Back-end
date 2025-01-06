using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class OrderList
    {

        public Guid OrderId { get; set; }
        public string Version { get; set; }

        public string? OrderNumber { get; set; }
        public string? Contact { get; set; }
        public Guid EstimateId { get; set; }
        public Guid? JobId { get; set; }
        public string? EstimateName { get; set; }
        public string? EstimateCode { get; set; }
        public Guid[]? WindowProductId { get; set; } = new Guid[] { };
        public string? TermsCondition { get; set; }
        public string? Notes { get; set; }
        public string? DiscountCoupon { get; set; }
        public string? Status { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Discount { get; set; }
        public decimal? GrandTotal { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }

        public string? primarycolor { get; set; }
        public string? TenantId { get; set; }

        public JobDetail jobDetails { get; set; } = new JobDetail();
        public List<EstimateProductLists> EstimateProductList { get; set; } = new List<EstimateProductLists>();
    }

    public class JobDetail
    {
        public string? JobName { get; set; }
        public string? JobType { get; set; }
        public string? Address { get; set; }
        public string? AddressLabel { get; set; }
        public string? AssignedPersonnel { get; set; }
        public string? JobNotes { get; set; }
        public DateTime? ScheduleTime { get; set; }

    }

    public class EstimateWindowLists
    {
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? ArchHeight { get; set; }
    }
    public class EstimateProductLists
    {
        public Guid EstimateWindowProductId { get; set; }
        public string? Label { get; set; }
        public string? ProductType { get; set; }
        public string? ProductTypeName { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? ArchHeight { get; set; }
        public string? ProductName { get; set; }
        public Guid? WindowProductId { get; set; }

        public decimal? Price { get; set; }
        public EstimateWindowLists EstimateWindowDetails { get; set; } = new EstimateWindowLists();

        public string[] Options { get; set; } = [];
        //public string[] price { get; set; }
    }
    public class OptionLists
    {
        public string? OptionName { get; set; }
    }
}


