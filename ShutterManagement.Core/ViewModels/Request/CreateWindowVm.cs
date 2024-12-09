using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateWindow
    {
        public Guid? ContactId { get; set; }
        public Guid? AddressId { get; set; }
        public string? Label { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? ArchHeight { get; set; }
        public string? Photo { get; set; }
        public string? Notes { get; set; }
        public decimal Quantity { get; set; }
        public Guid? JobId { get; set; }
        public List<WindowProductVm> WindowProducts { get; set; } = new List<WindowProductVm>();
    }
    public class WindowProductVm
    {
        public Guid? WindowId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ProductTypeId { get; set; }
        public Guid[]? OptionTypeId { get; set; }
        public decimal? BuildWidth { get; set; }
        public decimal? BuildHeight { get; set; }
        public decimal AdditionalCost { get; set; }
    }
}

