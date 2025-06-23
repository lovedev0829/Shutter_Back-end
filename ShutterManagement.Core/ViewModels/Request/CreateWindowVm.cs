using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

