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
      //  [Required]
        public string? APIkey { get; set; }
      //  [Required]
        public string? PipelineId { get; set; }
      //  [Required]
        public string? LocationId { get; set; }
      //  [Required]
        public string? BaseAddress { get; set; }
       // [Required]
        public string? GeneratedToken { get; set; }
        public string? CalendarName { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? RedirectURI { get; set; }
    }
}
