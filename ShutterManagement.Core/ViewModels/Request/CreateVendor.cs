using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateVendor
    {
        public string? VendorName { get; set; }
        public string? ContactInformation { get; set; }
        public bool? PreferredVendor { get; set; }
        public string? Notes { get; set; }
    }
}
