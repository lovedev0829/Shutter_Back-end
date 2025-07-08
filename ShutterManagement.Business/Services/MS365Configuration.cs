

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
        private readonly string _msOAuthGrantTypeAuthorization = "authorization_code";
        private readonly string _msOAuthGrantTypeRefresh = "refresh_token";
        private readonly string _msTokenInfoUrl = "https://www.googleapis.com/oauth2/v1/tokeninfo?access_token=";
        private readonly string _msTokenUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token";

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
                    { "response_type", "code" },
                    { "redirect_uri", AppConstant.Microsoft365_RedirectUri },
                    { "response_mode", "query" },
                    { "scope", AppConstant.Microsoft365_Scopes },
                    { "state", Guid.NewGuid().ToString() }
                };

                string authUrl = $"{AppConstant.Microsoft365_AuthUri}?" + 
                 string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

                responseVm.IsSuccess = true;
                responseVm.Message = "Authorization URL generated successfully.";
                responseVm.Data = authUrl;
            }
            catch (Exception ex)
            {
                responseVm.Message = "Error while generating Authorization URL: " + ex.Message;
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
                { "code", code },
                { "grant_type", _msOAuthGrantTypeAuthorization },
                { "redirect_uri", AppConstant.Microsoft365_RedirectUri },
                { "scope", AppConstant.Microsoft365_Scopes }
                };

                using var client = new HttpClient();
                var response = await client.PostAsync(
                    $"{_msTokenUrl}",
                    new FormUrlEncodedContent(tokenRequest)
                );

                var json = await response.Content.ReadAsStringAsync();

                //FileLogger.Log($"Response after authentication of MS365 {json}");
                //FileLogger.Log($"Response after authentication of MS365 {response}");

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
                //FileLogger.Log($"TokenResposnse after authentication of MS365 {tokenResult}");
                var config = (await emailConfigurationService.GetAll()).
                    Where(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false)
                    .FirstOrDefault();
                //FileLogger.Log($"Configuration of Email {config}");
                if (config != null)
                {
                    config.AccessToken = tokenResult.AccessToken;
                    config.RefreshToken = tokenResult.RefreshToken;
                    config.AccessExpirationtime = DateTime.Now.AddHours(1);
                    config.RefreshExpirationtime = DateTime.Now.AddDays(90);
                    var res = await emailConfigurationService.Update(config);
                   // FileLogger.Log($"Configuration added in Database {res}");
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

        //public async Task<IResponseVm<string>> SendEmail(EmailMessageModel message)
        //{
        //    IResponseVm<string> responseVm = new ResponseVm<string>();
        //    try
        //    {
        //        var email = new MimeMessage();
        //        email.From.Add(MailboxAddress.Parse(message.From));
        //        email.To.Add(MailboxAddress.Parse(message.To));
        //        email.Subject = message.Subject;

        //        var multipart = new Multipart("mixed");

        //        // Add body as first part
        //        multipart.Add(new TextPart("plain") { Text = message.Body });

        //        var config = emailConfigurationService.GetAllQuerable()
        //            .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false);

        //        if (message.Attachments != null && message.Attachments.Any())
        //        {
        //            foreach (var file in message.Attachments)
        //            {
        //                using var stream = new MemoryStream();
        //                await file.CopyToAsync(stream);
        //                stream.Position = 0;

        //                var attachment = new MimePart()
        //                {
        //                    Content = new MimeContent(stream, ContentEncoding.Default),
        //                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
        //                    ContentTransferEncoding = ContentEncoding.Base64,
        //                    FileName = file.FileName
        //                };

        //                multipart.Add(attachment);
        //            }
        //        }

        //        email.Body = multipart;

        //        if (config == null)
        //        {
        //            responseVm.Message = "Email configuration not found.";
        //            return responseVm;
        //        }

        //        using (var client = new SmtpClient())
        //        {
        //            await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
        //            var oauth2 = new SaslMechanismOAuth2(config.Username, config.AccessToken);
        //            await client.AuthenticateAsync(oauth2);
        //            await client.SendAsync(email);
        //            await client.DisconnectAsync(true);
        //        }
        //        responseVm.IsSuccess = true;
        //        responseVm.Message = "Email sent to " + message.To;
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = ex.Message;
        //    }
        //    return responseVm;
        //}

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
                ["grant_type"] = _msOAuthGrantTypeRefresh,
                ["refresh_token"] = refreshToken,
                ["scope"] = AppConstant.Microsoft365_Scopes
            };

            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(
                $"{_msTokenUrl}",
                content);

            var json = await response.Content.ReadAsStringAsync();
            dynamic tokenResult = JsonConvert.DeserializeObject(json);
            if(tokenResult != null && !string.IsNullOrEmpty(tokenResult.access_token))
            return tokenResult?.access_token;

            return string.Empty;
        }


        public async Task<IResponseVm<EmailConfigurationVm?>> ValidateAndRefreshMSEmailOauthAccessToken(TblEmailConfiguration emailConfig, string EmailType)
        {
            IResponseVm<EmailConfigurationVm> responseVm = new ResponseVm<EmailConfigurationVm>();

            try
            {
                // If refresh token expired
                if (emailConfig.RefreshExpirationtime <= DateTime.Now)
                {
                    responseVm.Message = AppConstant.AuthenticationExpiry;
                    return responseVm;
                }

                bool isAccessTokenExpired = emailConfig.AccessExpirationtime <= DateTime.Now;
                bool isAccessTokenValid = false;

                isAccessTokenValid = await IsMicrosoftAccessTokenValid(emailConfig.AccessToken ?? "");

                if (!isAccessTokenValid || isAccessTokenExpired)
                {
                    var newAccessToken = await RefreshMicrosoft365AccessToken(emailConfig.RefreshToken ?? "");

                    if (string.IsNullOrEmpty(newAccessToken))
                    {
                        responseVm.Message = "Failed to refresh Google access token.";
                        return responseVm;
                    }

                    emailConfig.AccessToken = newAccessToken;
                    emailConfig.AccessExpirationtime = DateTime.Now.AddHours(1);
                    await emailConfigurationService.Update(emailConfig);

                    responseVm.Message = "Google access token refreshed.";

                }
                responseVm = EmailConfigurationVm.ConfigResponse(emailConfig, EmailType);
                return responseVm;

            }
            catch (Exception ex)
            {
                responseVm.Message = "Exception: " + ex.Message;
                return responseVm;
            }
        }

    }


}
