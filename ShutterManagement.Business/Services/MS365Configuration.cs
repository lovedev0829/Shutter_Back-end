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

namespace ShutterManagement.Business.Services
{
    public class MS365Configuration
    {
        private readonly IEmailConfigurationService emailConfigurationService;
        private readonly ClaimsProperty claimsProperty;
        private readonly string _smtpServer = "smtp.office365.com";
        private readonly int _smtpPort = 587;
        private readonly string _userEmail;
        private readonly string _accessToken;
        public MS365Configuration(IEmailConfigurationService emailConfigurationService,ClaimsProperty claimsProperty)
        {
            this.emailConfigurationService = emailConfigurationService;
            this.claimsProperty = claimsProperty;
        }

        public async Task<IResponseVm<string>> GetAuthorizationurl()
        {
            IResponseVm<string> responseVm = new ResponseVm<string>();
            try
            {
                var query = new Dictionary<string, string>
             {
             { "client_id", AppConstant.Micorsoft365_ClientId },
             { "client_secret", AppConstant.Micorsoft365_ClientSecret },
             { "response_type", "code" },
             { "redirect_uri", AppConstant.Micorsoft365_RedirectUri },
             { "response_mode", "query" },
             { "scope", "https://outlook.office365.com/SMTP.Send offline_access" },
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
                { "client_id", AppConstant.Micorsoft365_ClientId },
                { "client_secret", AppConstant.Micorsoft365_ClientSecret },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", AppConstant.Micorsoft365_RedirectUri },
                { "scope", "https://outlook.office365.com/SMTP.Send offline_access" }
                };

                using var client = new HttpClient();
                var response = await client.PostAsync(
                    $"https://login.microsoftonline.com/Common/oauth2/v2.0/token",
                    new FormUrlEncodedContent(tokenRequest)
                );

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                   
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(json);
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
                var config = (await emailConfigurationService.GetAll()).
                    Where(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false)
                    .FirstOrDefault();
                if (config != null)
                {
                    config.AccessToken = tokenResult.AccessToken;
                    config.RefreshToken = tokenResult.RefreshToken;
                    config.AccessExpirationtime = DateTime.Now.AddHours(1);
                    config.RefreshExpirationtime = DateTime.Now.AddHours(1);
                    await emailConfigurationService.Update(config);
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
                    .FirstOrDefault(x => x.TenantId == AppConstant.LoggedInTenantId && x.IsActive == true && x.IsDelete == false);
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
                ["client_id"] = AppConstant.Micorsoft365_ClientId,
                ["client_secret"] = AppConstant.Micorsoft365_ClientSecret,
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
                ["scope"] = "https://outlook.office365.com/SMTP.Send offline_access"
            };

            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(
                $"https://login.microsoftonline.com/{AppConstant.Micorsoft365_tenantId}/oauth2/v2.0/token",
                content);

            var json = await response.Content.ReadAsStringAsync();
            dynamic tokenResult = JsonConvert.DeserializeObject(json);
            return tokenResult?.access_token;
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
