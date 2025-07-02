using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateEmailConfiguration
    {
        [DefaultValue("")]
        public string? SmtpServer { get; set; }

        [DefaultValue(0)]
        public int? Port { get; set; }

        [DefaultValue("")]
        public string? Username { get; set; }

        [DefaultValue("")]
        public string? Password { get; set; }

        public bool? UseSsl { get; set; }

        [DefaultValue("")]
        public string? ReplyTo { get; set; }

        [DefaultValue(null)]
        public Guid? EmailType { get; set; }

        public string? From { get; set; }

        public string? AccessToken { get; set; }

    }
}
