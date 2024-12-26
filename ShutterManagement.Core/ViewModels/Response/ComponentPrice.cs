using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class ComponentPrice
    {
        public Guid Id { get; set; }
        public string? ProductName { get; set; }
        public string? ComponentName { get; set; }
        public decimal PriceRated { get; set; }
        public decimal PriceNonRated { get; set; }
        public string? Name { get; set; }
    }
}
