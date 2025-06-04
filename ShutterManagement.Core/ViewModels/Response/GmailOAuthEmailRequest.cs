using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GmailOAuthEmailRequest
    {
        public string? FromEmail { get; set; }
        public string? ToEmail { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? UserEmail { get; set; } 
        public string? AccessToken { get; set; }
        public List<EmailAttachments>? Attachments { get; set; }
    }
    public class EmailAttachments
    {
        public Stream Stream;
        public string FileName;
        public string MediaType;

    }
}


