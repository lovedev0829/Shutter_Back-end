using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class SendEmail
    {
        public string? ToEmail { get; set; }
        public string? Body { get; set; }
        public string? Subject { get; set; }
    }
}
