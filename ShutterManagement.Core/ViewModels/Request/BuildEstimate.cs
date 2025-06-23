using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class BuildEstimate
    {
        public Guid[]? WindowProductIds { get; set; }
        public string? EstimateId { get; set; } = "";
        public bool IsBreakDown { get; set; } = false;
        public bool IsMaterialBreakdown { get; set; } = false;
        public bool IsView { get; set; } = false;
    }
}
