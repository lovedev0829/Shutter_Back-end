using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core;
using Azure;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.DAL.Context;

namespace ShutterManagement.Business.Services
{
    public class EmailSendHelperService 
    {
        private readonly MS365Configuration mS365Configuration;
        private readonly GoogleAuthService googleAuthService;
        private readonly IEmailConfigurationService emailConfigurationService;
        private readonly EmailTypeService emailTypeService;
        private readonly ClaimsProperty claimsProperty;

        public EmailSendHelperService(MS365Configuration mS365Configuration,GoogleAuthService googleAuthService,
            IEmailConfigurationService emailConfigurationService, EmailTypeService emailTypeService, ClaimsProperty claimsProperty)
        {
            this.mS365Configuration = mS365Configuration;
            this.googleAuthService = googleAuthService;
            this.emailConfigurationService = emailConfigurationService;
            this.emailTypeService = emailTypeService;
            this.claimsProperty = claimsProperty;
        }
        public async Task<EmailConfigurationVm?> GetEmailConfigurationByTypeAsync()
        {
            IResponseVm<EmailConfigurationVm?> configResponse = new ResponseVm<EmailConfigurationVm?>();
            var emailConfig = (await emailConfigurationService.GetAll())
                   .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
            var emailType = (await emailTypeService.GetAll())
                    .FirstOrDefault(s => s.Id == emailConfig?.EmailType)?.Name ?? "";
                   

            if (emailType.ToLower() == EnumValue.GetEnumValue(EmailSendType.Google).ToLower())
            {
                configResponse = await googleAuthService.ValidateAndRefreshGoogleEmailOauthAccessToken(emailConfig);
            }
            else if (emailType.ToLower() == EnumValue.GetEnumValue(EmailSendType.Microsoft365).ToLower())
            {
                configResponse = await mS365Configuration.ValidateAndRefreshMSEmailOauthAccessToken(emailConfig);
            }
            else if(emailType.ToLower() == EnumValue.GetEnumValue(EmailSendType.SMTP))
            {
                configResponse = SuccessResponse(emailConfig, emailType);
            }
            else
            {
                configResponse.IsSuccess = false;
                configResponse.Message = "Unsupported email type.";
                return null;
            }

            if (!configResponse.IsSuccess || configResponse.Data == null)
            {
                configResponse.IsSuccess = false;
                configResponse.Message = configResponse.Message ?? "Email configuration not found. Please add configurations.";
                return null;
            }

            return configResponse.Data;
        }
        private IResponseVm<EmailConfigurationVm> SuccessResponse(TblEmailConfiguration emailConfig, string emailType)
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
        public async Task<IResponseVm<string>> WelcomeEmailWithOauth(string Email, string content)
        {

            // store emailconfig response EmailConfigurationVm 
            var lowerEmailType = EmailType.ToLower();
            EmailConfigurationVm config = await GetEmailConfigurationByTypeAsync(lowerEmailType);

            OAuthEmailRequest vm = new OAuthEmailRequest
            {
                FromEmail = config?.Username ?? "",
                ToEmail = Email,
                Subject = AppConstant.Sub_WelcomeEmail,
                Body = content,
                UserEmail = config.Username ?? "",
                Username = config.Username ?? "",
                SmtpServer = config.SmtpServer ?? "",
                Port = config.Port ?? 0,
                AccessToken = config.AccessToken ?? "",
                Password = config.Password ?? "",
                UseSsl = config.UseSsl ?? true,
                EmailType = config.EmailTypeName ?? "",
                Attachments = null
            };

            return await EmailHelper.SendEmailWithOAuth(vm);
        }
    }
}
