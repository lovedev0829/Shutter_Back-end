using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetGHLConfiguration
    {
        public Guid? Id { get; set; }
        public string? APIkey { get; set; }
        public string? PipelineId { get; set; }
        public string? LocationId { get; set; }
        public string? BaseAddress { get; set; }
        public DateTime? LastSync { get; set; }
        public string? GeneratedToken { get; set; }
        public string? CalendarName { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? RedirectURI { get; set; }
        public string? Refresh_Token { get; set; }
        public DateTime? RefreshExpirationtime { get; set; }
        public string? Access_Token { get; set; }
        public DateTime? AccessExpirationtime { get; set; }
        public bool? IsConnected { get; set; }
    
    }
}
