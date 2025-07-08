using Azure;
using Newtonsoft.Json.Linq;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class EmailSendHelperService 
    {
        private readonly MS365Configuration mS365Configuration;
        private readonly GoogleAuthService googleAuthService;
        private readonly IEmailConfigurationService emailConfigurationService;
        private readonly ShutterDBContext dBContext;
        private readonly EmailTypeService emailTypeService;
        private readonly ClaimsProperty claimsProperty;

        public EmailSendHelperService(MS365Configuration mS365Configuration,GoogleAuthService googleAuthService,
            IEmailConfigurationService emailConfigurationService,ShutterDBContext dBContext, EmailTypeService emailTypeService, ClaimsProperty claimsProperty)
        {
            this.mS365Configuration = mS365Configuration;
            this.googleAuthService = googleAuthService;
            this.emailConfigurationService = emailConfigurationService;
            this.dBContext = dBContext;
            this.emailTypeService = emailTypeService;
            this.claimsProperty = claimsProperty;
        }


        public async Task<IResponseVm<string>> GetTemplateContent(EmailTemplateType emailTemplateType, dynamic dataObject)
        {
            IResponseVm<string> response = new ResponseVm<string>();
            try
            {
                var Template = dBContext.TblTemplates.FirstOrDefault(
                        x => x.IsDelete == false &&
                            x.TemplateCategory == EnumValue.GetEnumValue(emailTemplateType));
                if (Template == null)
                {
                    response.Message = "Email template not found. Please add the template.";
                    //throw new Exception("Email template not found. Please add the template.");
                    return response;
                }
                var emailContent = PlaceholderReplace(Template.TemplateValue, dataObject);
                response.Data = emailContent ?? "";

            }
            catch (Exception ex)
            {
                response.Message = "Error in GetTemplateContent: " + ex.Message;
                FileLogger.Log(claimsProperty.GetTenantId().ToString() + " Error in GetTemplateContent: " + ex.Message);
            }
            return response;
        }
        public async Task<IResponseVm<string>> SendEmailWithReplacedContent(EmailTemplateType emailTemplateType, dynamic dataObject)
        {
            IResponseVm<string> response = new ResponseVm<string>();
            try
            {
                var emailContent = await GetTemplateContent(emailTemplateType,dataObject);

                response = await GetEmailConfigAndSendEmail(dataObject.Email, emailContent ,AppConstant.Sub_WelcomeEmail );
            }
            catch (Exception ex)
            {
                response.Message = "Error in GetTemplateContentWithValues: " + ex.Message;
                FileLogger.Log(claimsProperty.GetTenantId().ToString() + " Error in GetTemplateContentWithValues: " + ex.Message);
            }
            return response;
        }

        public async Task<IResponseVm<string>> GetEmailConfigAndSendEmail(string ToEmail, string emailContent, string subject = "", string replyTo = "", List<EmailAttachments> emailAttachments = null)
        {
            IResponseVm<string> response = new ResponseVm<string>();
            try
            {
                var emailTypeconfiguration = await GetEmailConfigurationByType();

            if (!emailTypeconfiguration.IsSuccess)
            {
                response.Message = emailTypeconfiguration.Message ?? "Failed to get email configuration.";
                return response;
            }


            OAuthEmailRequest vm = new OAuthEmailRequest
            {
                FromEmail = emailTypeconfiguration.Data.Username ?? "",
                ToEmail = ToEmail,
                Subject = subject,
                Body = emailContent,
                ReplyToEmail = string.IsNullOrEmpty(replyTo) ? emailTypeconfiguration.Data.Username : replyTo,
                UserEmail = emailTypeconfiguration.Data.Username ?? "",
                Username = emailTypeconfiguration.Data.Username ?? "",
                SmtpServer = emailTypeconfiguration.Data.SmtpServer ?? "",
                Port = emailTypeconfiguration.Data.Port ?? 0,
                AccessToken = emailTypeconfiguration.Data.AccessToken ?? "",
                Password = emailTypeconfiguration.Data.Password ?? "",
                UseSsl = emailTypeconfiguration.Data.UseSsl ?? true,
                EmailType = emailTypeconfiguration.Data.EmailTypeName ?? "",
                Attachments = emailAttachments
            };
            response = await EmailHelper.SendEmail(vm);
            }
            catch (Exception ex)
            {
                response.Message = "Error in GetTemplateContentWithValues: " + ex.Message;
                FileLogger.Log(claimsProperty.GetTenantId().ToString() + " Error in GetTemplateContentWithValues: " + ex.Message);
            }
            return response;
        }
        public async Task<IResponseVm<EmailConfigurationVm>> GetEmailConfigurationByType()
        {
            IResponseVm<EmailConfigurationVm?> configResponse = new ResponseVm<EmailConfigurationVm?>();
            try
            {
                var emailConfig = (await emailConfigurationService.GetAll())
                       .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
                var emailType = (await emailTypeService.GetAll())
                        .FirstOrDefault(s => s.Id == emailConfig?.EmailType)?.Name ?? "";


                if (emailType.ToLower() == EnumValue.GetEnumValue(EmailSendType.Google).ToLower())
                {
                    configResponse = await googleAuthService.ValidateAndRefreshGoogleEmailOauthAccessToken(emailConfig, emailType);
                }
                else if (emailType.ToLower() == EnumValue.GetEnumValue(EmailSendType.Microsoft365).ToLower())
                {
                    configResponse = await mS365Configuration.ValidateAndRefreshMSEmailOauthAccessToken(emailConfig, emailType);
                }
                else if (emailType.ToLower() == EnumValue.GetEnumValue(EmailSendType.SMTP))
                {
                    configResponse = EmailConfigurationVm.ConfigResponse(emailConfig, emailType);
                }
                else
                {
                    FileLogger.Log(claimsProperty.GetTenantId().ToString() + " Unsupported email type.");
                    configResponse.Message = "Unsupported email type.";
                }
            }
            catch (Exception ex) {
                configResponse.Message = ex.Message;
                FileLogger.Log(claimsProperty.GetTenantId().ToString() + " Error in Fetch Email Config: " + ex.Message);
            }
            return configResponse;
        
        }

        public string PlaceholderReplace(string content, dynamic dataObject)
        {
            try
            {
                const string pattern = "{{.*?}}";
                var matches = Regex.Matches(content ?? string.Empty, pattern);
                foreach (Match match in matches)
                {
                    var placeholder = match.Value;
                    var property = placeholder.Replace("{{", "").Replace("}}", "");
                    object val = GetProperty.GetColumnValue(dataObject, property);

                    if (val != null)
                        content = content.Replace(placeholder, val.ToString() ?? string.Empty);

                    else
                        content = content.Replace(placeholder, string.Empty);
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log(claimsProperty.GetTenantId().ToString() + " Error in PlaceholderReplace: " + ex.Message);
                //throw new Exception("Error in PlaceholderReplace: " + ex.Message);
            }

            return content;
        }
    }
}
