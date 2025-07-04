

using Microsoft.Identity.Client;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Linq;
using System.Threading.Tasks;
using ShutterManagement.GHLv2;
using Newtonsoft.Json;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;

namespace ShutterManagement.Business.Services
{
    public class MS365Configuration
    {
        private readonly IEmailConfigurationService emailConfigurationService;
        private readonly ClaimsProperty claimsProperty;
        private readonly EmailTypeService emailTypeService;

        public MS365Configuration(IEmailConfigurationService emailConfigurationService,ClaimsProperty claimsProperty ,EmailTypeService emailTypeService)
        {
            this.emailConfigurationService = emailConfigurationService;
            this.claimsProperty = claimsProperty;
            this.emailTypeService = emailTypeService;
        }

        public async Task<IResponseVm<string>> GetAuthorizationurl()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var query = new Dictionary<string, string>
             {
             { "client_id", AppConstant.Microsoft365_ClientId },
             { "client_secret", AppConstant.Microsoft365_ClientSecret },
             { "response_type", "code" },
             { "redirect_uri", AppConstant.Microsoft365_RedirectUri },
             { "response_mode", "query" },
             { "scope", "SMTP.Send offline_access User.Read Mail.Send Mail.ReadWrite" },
             { "state", Guid.NewGuid().ToString() }
                };

                string authUrl = $"https://login.microsoftonline.com/Common/oauth2/v2.0/authorize?" + string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

                responseVm.IsSuccess = true;
                responseVm.Message = "Authorization URL generated successfully.";
                responseVm.Data = authUrl;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = "Error while generating Authorization URL: " + ex.Message;
                responseVm.Data = null;
            }


            return responseVm;
        }

        public async Task<IResponseVm<string>> ExchangeCodeForTokensAsync(string code)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var tokenRequest = new Dictionary<string, string>
                {
                { "client_id", AppConstant.Microsoft365_ClientId },
                { "client_secret", AppConstant.Microsoft365_ClientSecret },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", AppConstant.Microsoft365_RedirectUri },
                { "scope", "https://outlook.office365.com/SMTP.Send offline_access User.Read" }
                };

                using var client = new HttpClient();
                var response = await client.PostAsync(
                    $"https://login.microsoftonline.com/{AppConstant.Microsoft365_TenantId}/oauth2/v2.0/token",
                    new FormUrlEncodedContent(tokenRequest)
                );

                var json = await response.Content.ReadAsStringAsync();

                FileLogger.Log($"Response after authentication of MS365 {json}");
                FileLogger.Log($"Response after authentication of MS365 {response}");

                if (!response.IsSuccessStatusCode)
                {
                   
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(json);
                        FileLogger.Log($"Response after authentication of MS365 {errorResponse}");

                        string errorDescription = errorResponse?.error_description != null ? (string)errorResponse.error_description : "Unknown error occurred.";

                        responseVm.Message = $"Token exchange failed: {errorDescription}";
                    }
                    catch
                    {
                        responseVm.Message = $"Token exchange failed: {json}";
                    }

                    return responseVm;
                }

                var tokenResult = JsonConvert.DeserializeObject<TokenResponse>(json);
                FileLogger.Log($"TokenResposnse after authentication of MS365 {tokenResult}");
                var config = (await emailConfigurationService.GetAll()).
                    Where(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false)
                    .FirstOrDefault();
                FileLogger.Log($"Configuration of Email {config}");
                if (config != null)
                {
                    config.AccessToken = tokenResult.AccessToken;
                    config.RefreshToken = tokenResult.RefreshToken;
                    config.AccessExpirationtime = DateTime.Now.AddHours(1);
                    config.RefreshExpirationtime = DateTime.Now.AddHours(1);
                    var res = await emailConfigurationService.Update(config);
                    FileLogger.Log($"Configuration added in Database {res}");
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Microsoft 365 Email Configuration done Successfully.";
                }
                else
                {
                    responseVm.Message = "Add Email Cofiguration for Microsoft 365.";
                }
            }
            catch (Exception ex)
            {
                responseVm.Message = ex.Message;
            }

                return responseVm;
            }

        public async Task<IResponseVm<string>> SendEmail(EmailMessageModel message)
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(message.From));
                email.To.Add(MailboxAddress.Parse(message.To));
                email.Subject = message.Subject;
                email.Body = new TextPart("plain") { Text = message.Body };
                var multipart = new Multipart("mixed");
                
                multipart.Add(new TextPart("plain") { Text = message.Body });
                var config = emailConfigurationService.GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);
                if (message.Attachments != null && message.Attachments.Any())
                {
                    foreach (var file in message.Attachments)
                    {
                        using var stream = new MemoryStream();
                        await file.CopyToAsync(stream);
                        stream.Position = 0;

                        var attachment = new MimePart()
                        {
                            Content = new MimeContent(stream, ContentEncoding.Default),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = file.FileName
                        };

                        multipart.Add(attachment);
                    }
                }

                email.Body = multipart;
                if (config == null)
                    throw new InvalidOperationException("Email configuration not found.");
                var username = "randyh@gccsla.com";
                using var client = new MailKit.Net.Smtp.SmtpClient();

                await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
                var oauth2 = new SaslMechanismOAuth2(username, config.AccessToken);
                await client.AuthenticateAsync(oauth2);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);

                responseVm.IsSuccess = true;
                responseVm.Message = "Email sent to " + message.To;
            }
            catch(Exception ex)
            {
                responseVm.Message = ex.Message;
            }
            return responseVm;
        }



        public async Task<bool> IsMicrosoftAccessTokenValid(string accessToken)
        {
            using var client = new HttpClient();
            try
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> RefreshMicrosoft365AccessToken(string refreshToken)
        {
            var values = new Dictionary<string, string>
            {
                ["client_id"] = AppConstant.Microsoft365_ClientId,
                ["client_secret"] = AppConstant.Microsoft365_ClientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["scope"] = "https://outlook.office365.com/SMTP.Send offline_access"
            };

            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(
                $"https://login.microsoftonline.com/{AppConstant.Microsoft365_TenantId}/oauth2/v2.0/token",
                content);

            var json = await response.Content.ReadAsStringAsync();
            dynamic tokenResult = JsonConvert.DeserializeObject(json);
            return tokenResult?.access_token;
        }


        public async Task<IResponseVm<EmailConfigurationVm?>> ValidateAndRefreshMSEmailOauthAccessToken()
        {
            IResponseVm<EmailConfigurationVm> responseVm = new ResponseVm<EmailConfigurationVm>();

            try
            {
                var emailConfig = emailConfigurationService.GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

                if (emailConfig == null)
                {
                    responseVm.Message = "No email configuration found.";
                    return responseVm;
                }

                var emailType = emailTypeService.GetAllQuerable()
                    .Where(s => s.Id == emailConfig.EmailType)
                    .Select(x => x.Name ?? "")
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(emailType))
                {
                    responseVm.Message = "Email type is not defined.";
                    return responseVm;
                }

                var lowerEmailType = emailType.ToLower();

                if (emailType == EnumValue.GetEnumValue(EmailSendType.SMTP))
                {
                    // For SMTP, no token check needed, return directly
                    return SuccessResponse(emailConfig, emailType);
                }

                // If refresh token expired
                if (emailConfig.RefreshExpirationtime <= DateTime.Now)
                {
                    responseVm.Message = "Your authentication has expired. Please reconnect your email to continue sending emails.";
                    return responseVm;
                }

                bool isAccessTokenExpired = emailConfig.AccessExpirationtime <= DateTime.Now;
                bool isAccessTokenValid = false;

                isAccessTokenValid = await IsMicrosoftAccessTokenValid(emailConfig.AccessToken);

                if (!isAccessTokenValid || isAccessTokenExpired)
                {
                    var newAccessToken = await RefreshMicrosoft365AccessToken(emailConfig.RefreshToken);

                    if (string.IsNullOrEmpty(newAccessToken))
                    {
                        responseVm.Message = "Failed to refresh Microsoft 365 access token.";
                        return responseVm;
                    }

                    emailConfig.AccessToken = newAccessToken??"";
                    emailConfig.AccessExpirationtime = DateTime.Now.AddHours(1);
                    await emailConfigurationService.Update(emailConfig);

                    responseVm.Message = "Microsoft 365 access token refreshed.";
                }


                // Return success response
                return SuccessResponse(emailConfig, emailType);
            }
            catch (Exception ex)
            {
                responseVm.Message = "Exception: " + ex.Message;
                return responseVm;
            }
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

        public class EmailMessageModel
        {
            public string From { get; set; }
            public string To { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public List<IFormFile> Attachments { get; set; }
        }

    }


}
