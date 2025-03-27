using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetGHLConfiguration
    {
        public string? APIkey { get; set; }
        public string? PipelineId { get; set; }
        public string? LocationId { get; set; }
        public string? BaseAddress { get; set; }
        public DateTime? LastSync { get; set; }
        public string? GeneratedToken { get; set; }
        public string? CalendarName { get; set; }
    
    }
}
