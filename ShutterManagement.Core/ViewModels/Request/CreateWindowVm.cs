using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateWindow
    {

        //[Required]
        //public Guid? ContactId { get; set; }
        //[Required]
        public Guid? AddressId { get; set; }
        [Required]
        public string? Label { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        //public bool? IsRated { get; set; } = false;
        public decimal? ArchHeight { get; set; }
        public string[]? Photo { get; set; } = [];
        public string? Notes { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        public Guid? JobId { get; set; }
        public List<WindowProductVm> WindowProducts { get; set; } = new List<WindowProductVm>();
        public List<AdditionalCostData>? AdditionalCosts { get; set; } = new List<AdditionalCostData>();
        public List<ProductDetail>? ProductDetails { get; set; } = new List<ProductDetail>();
    }
    public class WindowProductVm
    {
        //public Guid? WindowId { get; set; }
        [Required(ErrorMessage ="Please select a product")]
        public Guid? ProductId { get; set; }
        [Required(ErrorMessage = "Please select a product type")]
        public Guid? ProductTypeId { get; set; }
        public List<string>? OptionTypeId { get; set; } = new List<string>();
        [Required(ErrorMessage = "Product width is required")]
        public decimal? BuildWidth { get; set; }
        [Required(ErrorMessage = "Product height is required")]
        public decimal? BuildHeight { get; set; }
        public decimal? AdditionalCost { get; set; }

        public int? SortOrder { get; set; }
    }

    public class AdditionalCostData
    {
        [JsonPropertyName("indexId")]
        public int IndexId { get; set; }
        
        [JsonPropertyName("data")]
        public List<AdditionalCostItem> Data { get; set; } = new List<AdditionalCostItem>();
    }

    public class AdditionalCostItem
    {
        [JsonPropertyName("optionTypeId")]
        public Guid? OptionTypeId { get; set; }
        
        [JsonPropertyName("optionGroupName")]
        public string? OptionGroupName { get; set; }
        
        [JsonPropertyName("optionType")]
        public string? OptionType { get; set; }
        
        [JsonPropertyName("additionalCost")]
        public decimal AdditionalCost { get; set; }
        
        [JsonPropertyName("requirePriceAtEstimatePreview")]
        public bool RequirePriceAtEstimatePreview { get; set; }
    }

    public class ProductDetail
    {
        public Guid WindowProductId { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid ProductTypeId { get; set; }
        public string? ProductTypeName { get; set; }
        public decimal BuildWidth { get; set; }
        public decimal BuildHeight { get; set; }
        public List<WindowOptionDetail>? WindowOptions { get; set; } = new List<WindowOptionDetail>();
    }

    public class WindowOptionDetail
    {
        public string? WindowoptionId { get; set; } // Primary identifier for existing window options
        public Guid WindowProductId { get; set; }
        public string? OptionTypeId { get; set; }
        public string? OptionName { get; set; }
        public decimal AdditionalCost { get; set; }
    }
}

