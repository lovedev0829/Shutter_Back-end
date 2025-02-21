using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class GHLConfiguration
    {
        public string? APIkey { get; set; }
        public string? PipelineId { get; set; }
        public string? StageId { get; set; }
        public string? BaseAddress { get; set; }
    }
}
