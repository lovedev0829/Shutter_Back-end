using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class WindowsList
    {
        public Guid WindowId { get; set; }
        public Guid? AddressId { get; set; }
        public string? Label { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? ArchHeight { get; set; }
        public string? Photo { get; set; }
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }
        public Guid? TenantId { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid ProductTypeId { get; set; }
        public string? ProductTypeName { get; set; }
        public string? OptionTypeId { get; set; }
        public string? OptionName { get; set; }
        public decimal? BuildWidth { get; set; }
        public decimal? BuildHeight { get; set; }
    }
}
