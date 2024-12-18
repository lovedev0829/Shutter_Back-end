using System;
using System.Collections.Generic;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class WindowsList
    {
        public  Guid? JobId { get; set; }
        public Guid WindowId { get; set; }
        public Guid? AddressId { get; set; }
        public string? Label { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? ArchHeight { get; set; }
        public string[]? Photo { get; set; } = new string[] {};
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }
        public Guid? TenantId { get; set; }
        public List<ProductList>? ProductDetails { get; set; }
    }

    public class ProductList
    {
        public Guid WindowProductId { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid? ProductTypeId { get; set; }
        public string? ProductTypeName { get; set; }
        public decimal? BuildWidth { get; set; }
        public decimal? BuildHeight { get; set; }

        public List<WindowOptions>? WindowOptions { get; set; }
    }

    public class WindowOptions
    {
        public Guid WindowProductId { get; set; }
        public string OptionTypeId { get; set; }
        public string OptionName { get; set; }
        public decimal AdditionalCost { get; set; }
    
    }
}
