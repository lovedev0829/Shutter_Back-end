using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class EstimateDetails
    {

            public Guid EstimateId { get; set; }
            public Guid? JobId { get; set; }
            public string? EstimateName { get; set; }
            public string? EstimateCode { get; set; }
            public decimal? TotalPrice { get; set; }
            public decimal? DiscountAmount { get; set; }
            public decimal? TaxAmount { get; set; }
            public decimal? GrandTotal { get; set; }
            public string? Status { get; set; }
            public string? TermsCondition { get; set; }
            public string? Notes { get; set; }
            public string? DiscountCoupon { get; set; }
            public string? TenantId { get; set; }

        //TblEstimateWindow

            public Guid? EstWindowId { get; set; }
            public Guid? AddressId { get; set; }
            public string? Label { get; set; }
            public decimal? Width { get; set; }
            public decimal? Height { get; set; }
            public decimal? ArchHeight { get; set; }
            public string? Photo { get; set; }
            public string? WindowNotes { get; set; }
            public decimal? Quantity { get; set; }
            public Guid? WindowId { get; set; }

           public  string? OWNotes { get; set; }

        //[TblEstimateWindowProduct]

        public Guid? EstWindowProductId { get; set; }
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductType { get; set; }
        public decimal? WPWidth { get; set; }
        public decimal? WPHeight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Area { get; set; }
        public decimal? AdditionalWidth { get; set; }
        public decimal? WPArchHeight { get; set; }
        public string? Color { get; set; }
        public string? WPNotes { get; set; }
        public decimal? Markup { get; set; }
        public Guid? WindowProductId { get; set; }

        //[TblEstimateWindowProductComponent]
        public Guid? EstWindowProductComponent { get; set; }
        public Guid? ComponentId { get; set; }
        public string? ComponentName { get; set; }
        public string? WCProductType { get; set; }
        public decimal? MarkupPercentage { get; set; }
        public string? PricingExpression { get; set; }
        public string? WCNotes { get; set; }

        //[TblEstimateWindowProductComponentMaterial]

        public Guid? EstWindowProductComponentMaterialId { get; set; }
        public Guid? RawMaterialId { get; set; }
        public string? MaterialName { get; set; }
        public decimal? UnitCost { get; set; }
        public string? UnitofMeasurement { get; set; }
        public decimal? StockQuantity { get; set; }
        public decimal? ReOrderlevel { get; set; }
        public Guid? VendorId { get; set; }
        public string? WRNotes { get; set; }
        public string? SKU { get; set; }
        public bool? IsCutSheet { get; set; }
        public Guid? WRProductType { get; set; }
    }

    //public class JobDetails
    //{
    //    public string? JobName { get; set; }
    //    public string? JobType { get; set; }
    //    public string? Address { get; set; }
    //    public string? AddressLabel { get; set; }
    //    public string? AssignedPersonnel { get; set; }
    //    public string? JobNotes { get; set; }
    //    public DateTime? ScheduleTime { get; set; }

    //}

    //public class EstimateWindowLists
    //    {
    //        public decimal? Height { get; set; }
    //        public decimal? Width { get; set; }
    //        public decimal? ArchHeight { get; set; }
    //    }
    //    public class EstimateProductLists
    //    {
    //        public Guid EstimateWindowProductId { get; set; }
    //        public string? Label { get; set; }
    //        public string? ProductType { get; set; }
    //        public string? ProductTypeName { get; set; }
    //        public decimal? Width { get; set; }
    //        public decimal? Height { get; set; }
    //        public decimal? ArchHeight { get; set; }
    //        public string? ProductName { get; set; }
    //        public Guid? WindowProductId { get; set; }

    //        //public decimal? ArchTop { get; set; }
    //        public EstimateWindowList EstimateWindowDetails { get; set; } = new EstimateWindowList();

    //        public List<OptionList> Options { get; set; } = new List<OptionList>();
    //        //public string[] price { get; set; }
    //    }
    //    public class OptionLists
    //    {
    //        public string? OptionName { get; set; }
    //    }
   
}
