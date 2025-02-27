using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class GHLConfiguration
    {
        [Required]
        public string? APIkey { get; set; }
        [Required]
        public string? PipelineId { get; set; }
        [Required]
        public string? StageId { get; set; }
        [Required]
        public string? BaseAddress { get; set; }
    }
}
