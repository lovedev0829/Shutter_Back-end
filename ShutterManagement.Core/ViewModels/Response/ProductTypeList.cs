using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class ProductTypeList
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }
}
