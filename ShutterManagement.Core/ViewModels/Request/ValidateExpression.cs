using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class ValidateExpression
    {
        public string? Expression { get; set; }
        public string? BuildWidth { get; set; }
        public string? BuildHeight { get; set; }
        public string? Optionprice { get; set; }
        public string? Unit { get; set; }
        public string? Quantity { get; set; }
        public bool? IsView { get; set; } = false;
        public bool? IsRated { get; set; } = true;
    }
}
