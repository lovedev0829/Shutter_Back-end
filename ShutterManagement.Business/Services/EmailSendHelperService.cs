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

namespace ShutterManagement.Business.Services
{
    public class EmailSendHelperService
    {
        private readonly MS365Configuration mS365Configuration;
        private readonly GoogleAuthService googleAuthService;

        public EmailSendHelperService(MS365Configuration mS365Configuration,GoogleAuthService googleAuthService)
        {
            this.mS365Configuration = mS365Configuration;
            this.googleAuthService = googleAuthService;
        }
        public async Task<EmailConfigurationVm?> GetEmailConfigurationByTypeAsync(string emailType)
        {
            IResponseVm<EmailConfigurationVm?> configResponse = new ResponseVm<EmailConfigurationVm?>();

            if (emailType.ToLower() == EnumValue.GetEnumValue(EmailSendType.Google).ToLower())
            {
                configResponse = await googleAuthService.ValidateAndRefreshGoogleEmailOauthAccessToken();
            }
            else if (emailType.ToLower() == EnumValue.GetEnumValue(EmailSendType.Microsoft365).ToLower())
            {
                configResponse = await mS365Configuration.ValidateAndRefreshMSEmailOauthAccessToken();
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

        public async Task<IResponseVm<string>> WelcomeEmailWithOauth(string Email, string content,string EmailType)
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
