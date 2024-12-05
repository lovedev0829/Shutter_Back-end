using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ShutterManagement.Core.ViewModels.Response
{
    public  class WindowListVm
    {
            public Guid WindowId { get; set; }
            public Guid? AddressId { get; set; }
            public string? Label { get; set; }
            public decimal? Width { get; set; }
            public decimal? Height { get; set; }
            public decimal? ArchHeight { get; set; }
            public string? Photo { get; set; }
            public string? Notes { get; set; }
            public bool? IsActive { get; set; }
            public Guid? TenantId { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public List<WindowProductVm> WindowProducts { get; set; } = new();
        }

        public class WindowProductVm
        {
            public Guid ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal? Quantity { get; set; }
            public bool? IsActive { get; set; }
        }

    }

