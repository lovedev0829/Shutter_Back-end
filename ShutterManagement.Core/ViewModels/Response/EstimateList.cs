using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class EstimateList
    {

        public Guid EstimateId { get; set; }
        public Guid? JobId { get; set; }
        public Guid? taxRateId { get; set; }
        public string? Region { get; set; }
        public decimal? TaxRate { get; set; }

        public string? EstimateName { get; set; }
        public decimal? TotalPrice { get; set; }
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
        public string? CompanyName { get; set; }

        public bool? IsTaxApplied { get; set; } = false;
        public JobDetails jobDetails { get; set; } = new JobDetails();
        public List<EstimateProductList> EstimateProductList { get; set; } = new List<EstimateProductList>();
    }

    public class  JobDetails
    {
        public string? JobName { get; set; }
        public string? JobType { get; set; }
        public string? Address { get; set; }
        public string? AddressLabel { get; set; }
        public string? AssignedPersonnel { get; set; }
        public string? JobNotes { get; set; }
        public string? JobStatus { get; set; }
        public DateTime? ScheduleTime { get; set; }

    }

    public class EstimateWindowList
    {
      
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? ArchHeight { get; set; }
    }
    public class EstimateProductList
    {
        public Guid EstimateWindowProductId { get; set; }

        public decimal? WindowQty { get; set; }
        public string? Label { get; set; }
        public string? ProductType { get; set; }
        public string? ProductTypeName { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? ArchHeight { get; set; }
        public string? ProductName { get; set; }
        public Guid? WindowProductId { get; set; }

        public decimal? TotalPrice { get; set; }
        public EstimateWindowList EstimateWindowDetails { get; set; } = new EstimateWindowList();

        public string[]? Options { get; set; } = [];
        //public string[] price { get; set; }
    }
    public class OptionList
    {
        public string? OptionName { get; set; }
    }
}
