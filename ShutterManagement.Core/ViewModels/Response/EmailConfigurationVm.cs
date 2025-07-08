using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class EmailConfigurationVm
    {
        public Guid Id { get; set; }

        public string? SmtpServer { get; set; }

        public int? Port { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public bool? UseSsl { get; set; }

        public string? ReplyTo { get; set; }

        public Guid? EmailType { get; set; }
        public string? EmailTypeName { get; set; }

        public string? From { get; set; }

        public string? AccessToken { get; set; }
        public Guid? TenantId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsConnected { get; set; }

        public DateTime? RefreshExpirationtime { get; set; }

        public DateTime? AccessExpirationtime { get; set; }

        public static IResponseVm<EmailConfigurationVm> ConfigResponse(TblEmailConfiguration emailConfig, string emailType)
        {
            return new ResponseVm<EmailConfigurationVm>
            {
                IsSuccess = true,
                Data = new EmailConfigurationVm
                {
                    Id = emailConfig.Id,
                    SmtpServer = emailConfig.SmtpServer,
                    Port = emailConfig.Port,
                    Username = emailConfig.Username,
                    Password = emailConfig.Password,
                    UseSsl = emailConfig.UseSsl,
                    ReplyTo = emailConfig.ReplyTo,
                    EmailType = emailConfig.EmailType,
                    EmailTypeName = emailType,
                    From = emailConfig.From,
                    AccessToken = emailConfig.AccessToken,
                    TenantId = emailConfig.TenantId,
                    IsActive = emailConfig.IsActive
                }
            };
        }
    
    }
}
