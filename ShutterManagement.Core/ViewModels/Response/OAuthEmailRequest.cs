using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class OAuthEmailRequest
    {
        public string? FromEmail { get; set; }
        public string? ToEmail { get; set; }
        public string? CC { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? UserEmail { get; set; } 
        public string? EmailType { get; set; } 
        public string? AccessToken { get; set; }

        public string? SmtpServer { get; set; }
        public string? ReplyToEmail { get; set; }

        public int Port { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public bool UseSsl { get; set; }
        public List<EmailAttachments>? Attachments { get; set; } = new List<EmailAttachments>();
    }
    public class EmailAttachments
    {
        public Stream Stream;
        public string FileName;
        public string MediaType;

    }
}


