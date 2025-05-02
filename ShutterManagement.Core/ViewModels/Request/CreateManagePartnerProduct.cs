using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace ShutterManagement.Core.ViewModels.Request
{

    public class CreateManagePartnerProduct
    {
        [DefaultValue("")]
        public Guid? ProductId { get; set; }
        public bool? IsAvailable { get; set; }
        [DefaultValue("")]
        public decimal? WholeSalePrice { get; set; }
        [DefaultValue("")]
        public decimal? MaxPartnerPrice { get; set; }
        [DefaultValue("")]
        public decimal? PartnerSellingPrice { get; set; }
        [DefaultValue("")]
        public string? TenantId { get; set; }
        public ManagePartnerProductOptions[]? ManagePartnerProductOptions { get; set; }
       // public ManagerPartnerProducts[]? managerPartnerProducts { get; set; }
    }

    //public class ManagerPartnerProducts
    //{
    //    [DefaultValue("")]
    //    public Guid? ProductId { get; set; }
    //    public bool? IsAvailable { get; set; }
    //    [DefaultValue("")]
    //    public decimal? WholeSalePrice { get; set; }
    //    [DefaultValue("")]
    //    public decimal? MaxPartnerPrice { get; set; }
    //    [DefaultValue("")]
    //    public decimal? PartnerSellingPrice { get; set; }
    //    [DefaultValue("")]
    //    public string? TenantId { get; set; }
    //    public ManagePartnerProductOptions[]? ManagePartnerProductOptions { get; set; }
    //}
    public class ManagePartnerProductOptions
    {
        [DefaultValue("")]
        public Guid? OptionId { get; set; }
        [DefaultValue("")]
        public Guid? OptionTypeId { get; set; }
        public bool? IsAvailable { get; set; }
        [DefaultValue("")]
        public decimal? WholeSalePrice { get; set; }
        [DefaultValue("")]
        public decimal? MaxPrice { get; set; }
        [DefaultValue("")]
        public decimal? PartnerSellingPrice { get; set; }
    }
}
