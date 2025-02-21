using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateProductType
    {
        public string? ProductName { get; set; }
        public bool? IsDefault { get; set; }
    }
}
