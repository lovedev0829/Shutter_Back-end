using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Newtonsoft.Json;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class GoogleAuthService : BaseRepository<ShutterDBContext, TblEmailType>
    {
        private readonly IConfiguration _config;
        private readonly IEmailConfigurationService emailConfigurationService;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenant;
        private readonly ShutterDBContext dBContext;
        private readonly MS365Configuration mS365Configuration;
        private readonly EmailTypeService emailTypeService;

        public GoogleAuthService(IConfiguration config,IEmailConfigurationService emailConfigurationService,
            ClaimsProperty claimsProperty,ITenantService tenant,ShutterDBContext dBContext,MS365Configuration mS365Configuration,EmailTypeService emailTypeService)
        {
            _config = config;
            this.emailConfigurationService = emailConfigurationService;
            this.claimsProperty = claimsProperty;
            this.tenant = tenant;
            this.dBContext = dBContext;
            this.mS365Configuration = mS365Configuration;
            this.emailTypeService = emailTypeService;
        }

        public string GetAuthUrl(string userEmail)
        {
            var query = new Dictionary<string, string>
            {
                ["client_id"] = _config["GoogleOAuth:ClientId"],
                ["redirect_uri"] = _config["GoogleOAuth:RedirectUri"],
                ["response_type"] = "code",
                ["scope"] = "https://mail.google.com/",
                ["access_type"] = "offline",
                ["prompt"] = "consent",
                ["login_hint"] = userEmail
            };

            return "https://accounts.google.com/o/oauth2/auth?" + string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        }


        public async Task<IResponseVm<string>> GenerateGoogleOAuthUrl()
        {
            var responseVm = new ResponseVm<string>();
            try
            {
                var existingEmailConfiguration = emailConfigurationService
                    .GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId());
                if (string.IsNullOrEmpty(existingEmailConfiguration.Username))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "User email is required Enter Username in configuration ";
                    return responseVm;
                }
                var clientId = _config["GoogleOAuth:ClientId"];
                var redirectUri = _config["GoogleOAuth:RedirectUri"];

                var scopes = new[]
                {
                "https://www.googleapis.com/auth/gmail.send",
                "https://www.googleapis.com/auth/gmail.labels",
                "https://mail.google.com/"
                };

                var scope = string.Join(" ", scopes);

                var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                              $"?client_id={clientId}" +
                              $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                              $"&response_type=code" +
                              $"&scope={Uri.EscapeDataString(scope)}" +
                              $"&access_type=offline" +
                              $"&prompt=consent" +
                              $"&login_hint={existingEmailConfiguration.Username}";
                responseVm.IsSuccess = true;
                responseVm.Message = "Google OAuth URL generated successfully";
                responseVm.Data = authUrl;
                return responseVm;
            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = "An error occurred while generating the Google OAuth URL: " + ex.Message;
                return responseVm;

            }

        }

        public async Task<IResponseVm<string>> ExchangeCodeForTokenAsync(string code)
        {
            var responseVm = new ResponseVm<string>();
            try
            {
                var values = new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["client_id"] = _config["GoogleOAuth:ClientId"],
                    ["client_secret"] = _config["GoogleOAuth:ClientSecret"],
                    ["redirect_uri"] = _config["GoogleOAuth:RedirectUri"],
                    ["grant_type"] = "authorization_code"
                };

                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {

                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(result);
                        string errorDescription = errorResponse?.error_description != null ? (string)errorResponse.error_description : "Unknown error occurred.";

                        responseVm.Message = $"Token exchange failed: {errorDescription}";
                    }
                    catch
                    {
                        responseVm.Message = $"Token exchange failed: {result}";
                    }

                    return responseVm;
                }

                var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(result);

                var config = emailConfigurationService
                    .GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId());

                if (config != null)
                {
                    config.AccessToken = tokenResponse.AccessToken;
                    config.RefreshToken = tokenResponse.RefreshToken;
                    config.AccessExpirationtime = DateTime.Now.AddHours(1);
                    config.RefreshExpirationtime = DateTime.Now.AddDays(7);

                    await emailConfigurationService.Update(config);
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Your accoount linked for email sending.";
                    return responseVm;
                }

            }
            catch (Exception ex)
            {
                responseVm.IsSuccess = false;
                responseVm.Message = "An error occurred while exchanging code for token: " + ex.Message;
                return responseVm;
            }
           
            return responseVm;

        }


        public async Task<string> GoogleRefreshAccessToken(string refreshToken)
        {
            var values = new Dictionary<string, string>
            {
                ["client_id"] = _config["GoogleOAuth:ClientId"],
                ["client_secret"] = _config["GoogleOAuth:ClientSecret"],
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            };

            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
            var result = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(result);

            return json.access_token;
        }

        public async Task<bool> IsGoogleAccessTokenValid(string accessToken)
        {
            using var client = new HttpClient();

            try
            {
                var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={accessToken}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(content);

                    // Optionally check if the token has the required scopes or client ID
                    return true;
                }
            }
            catch
            {
                // log error if needed
            }

            return false;
        }



        public async Task<IResponseVm<EmailConfigurationVm?>> ValidateAndRefreshEmailOauthAccessToken()
        {
            IResponseVm<EmailConfigurationVm> responseVm = new ResponseVm<EmailConfigurationVm>();

            try
            {
                var emailConfig = emailConfigurationService.GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive==true && x.IsDelete==false);

                if (emailConfig == null)
                {
                    responseVm.Message = "No email configuration found.";
                    return responseVm;
                }

                var emailType = emailTypeService.GetAllQuerable()
                    .Where(s => s.Id == emailConfig.EmailType)
                    .Select(x => x.Name??"")
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

                switch (lowerEmailType)
                {
                    case "google":
                        isAccessTokenValid = await IsGoogleAccessTokenValid(emailConfig.AccessToken);

                        if (!isAccessTokenValid || isAccessTokenExpired)
                        {
                            var newAccessToken = await GoogleRefreshAccessToken(emailConfig.RefreshToken);

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
                        break;

                    case "microsoft365":
                        isAccessTokenValid = await mS365Configuration.IsMicrosoftAccessTokenValid(emailConfig.AccessToken);

                        if (!isAccessTokenValid || isAccessTokenExpired)
                        {
                            var newAccessToken = await mS365Configuration.RefreshMicrosoft365AccessToken(emailConfig.RefreshToken);

                            if (string.IsNullOrEmpty(newAccessToken))
                            {
                                responseVm.Message = "Failed to refresh Microsoft 365 access token.";
                                return responseVm;
                            }

                            emailConfig.AccessToken = newAccessToken;
                            emailConfig.AccessExpirationtime = DateTime.Now.AddHours(1);
                            await emailConfigurationService.Update(emailConfig);

                            responseVm.Message = "Microsoft 365 access token refreshed.";
                        }
                        break;

                    default:
                        responseVm.Message = $"Unsupported Email Type: {emailType}";
                        return responseVm;
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

        // Helper to generate response
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



        //public async Task<IResponseVm<EmailConfigurationVm?>> ValidateAndRefreshEmailOauthAccessToken()
        //{

        //    IResponseVm<EmailConfigurationVm> responseVm = new ResponseVm<EmailConfigurationVm>();

        //    try
        //    {
        //        var emailConfig =  emailConfigurationService.GetAllQuerable()
        //            .Where(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive == true && x.IsDelete == false).FirstOrDefault();


        //        if (emailConfig == null)
        //        {
        //            responseVm.Message = "No email configuration found.";
        //            return responseVm;
        //        }




        //        var emailType = dBContext.TblEmailTypes.Where(s => s.Id == emailConfig.EmailType).Select(x => x.Name).FirstOrDefault();

        //        if (string.IsNullOrEmpty(emailType))
        //        {
        //            responseVm.Message = "Email type is not defined.";
        //            return responseVm;
        //        }
        //        if(emailType==EnumValue.GetEnumValue(EmailSendType.SMTP))
        //        {
        //            // it work without refeshh token or any other it work normally like skipp for check any expiration and get return emailConfig data 
        //        }

        //        if (emailConfig.RefreshExpirationtime <= DateTime.Now)
        //        {
        //            responseVm.Message = "Your authentication has expired. Please reconnect your email to continue sending emails.";
        //            return responseVm;
        //        }

        //        bool isAccessTokenExpired = emailConfig.AccessExpirationtime <= DateTime.Now;
        //        bool isAccessTokenValid = false;

        //        switch (emailType.ToLower())
        //        {
        //            case "google":
        //                isAccessTokenValid = await IsGoogleAccessTokenValid(emailConfig.AccessToken);
        //                if (!isAccessTokenValid || isAccessTokenExpired)
        //                {
        //                    var newAccessToken = await GoogleRefreshAccessToken(emailConfig.RefreshToken);
        //                    if (!string.IsNullOrEmpty(newAccessToken))
        //                    {
        //                        emailConfig.AccessToken = newAccessToken;
        //                        emailConfig.AccessExpirationtime = DateTime.Now.AddHours(1);
        //                        await emailConfigurationService.Update(emailConfig);
        //                        responseVm.Message = "Google access token refreshed.";
        //                    }
        //                    else
        //                    {
        //                        responseVm.Message = "Failed to refresh Google access token.";
        //                        return responseVm;
        //                    }
        //                }
        //                break;

        //            case "microsoft365":
        //                isAccessTokenValid = await mS365Configuration.IsMicrosoftAccessTokenValid(emailConfig.AccessToken);
        //                if (!isAccessTokenValid || isAccessTokenExpired)
        //                {
        //                    var newAccessToken = await mS365Configuration.RefreshMicrosoft365AccessToken(emailConfig.RefreshToken);
        //                    if (!string.IsNullOrEmpty(newAccessToken))
        //                    {
        //                        emailConfig.AccessToken = newAccessToken;
        //                        emailConfig.AccessExpirationtime = DateTime.Now.AddHours(1);
        //                        await emailConfigurationService.Update(emailConfig);
        //                        responseVm.Message = "Microsoft 365 access token refreshed.";
        //                    }
        //                    else
        //                    {
        //                        responseVm.Message = "Failed to refresh Microsoft 365 access token.";
        //                        return responseVm;
        //                    }
        //                }
        //                break;

        //            default:
        //                responseVm.Message = $"Unsupported Email Type: {emailType}";
        //                return responseVm;
        //        }

        //        var vm = new EmailConfigurationVm
        //        {
        //            Id = emailConfig.Id,
        //            SmtpServer = emailConfig.SmtpServer,
        //            Port = emailConfig.Port,
        //            Username = emailConfig.Username,
        //            Password = emailConfig.Password,
        //            UseSsl = emailConfig.UseSsl,
        //            ReplyTo = emailConfig.ReplyTo,
        //            EmailType = emailConfig.EmailType,
        //            EmailTypeName = emailType,
        //            From = emailConfig.From,
        //            AccessToken = emailConfig.AccessToken,
        //            TenantId = emailConfig.TenantId,
        //            IsActive = emailConfig.IsActive
        //        };

        //        responseVm.IsSuccess = true;
        //        responseVm.Data = vm;
        //    }
        //    catch (Exception ex)
        //    {
        //        responseVm.Message = "Exception: " + ex.Message;
        //    }

        //    return responseVm;
        //}




        //public async Task<TblEmailConfiguration?> ValidateAndRefreshGoogleAccessToken()
        //{

        //    var emailConfig = emailConfigurationService.GetAllQuerable()
        //        .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId() && x.IsActive==true && x.IsDelete==false);

        //    if (emailConfig == null)
        //        return null;

        //    bool isAccessTokenExpired = emailConfig.AccessExpirationtime <= DateTime.Now;
        //    bool isAccessTokenValid = await IsAccessTokenValid(emailConfig.AccessToken);

        //    if (!isAccessTokenValid || isAccessTokenExpired)
        //    {
        //        var newAccessToken = await RefreshAccessTokenAsync(emailConfig.RefreshToken);
        //        if (!string.IsNullOrEmpty(newAccessToken))
        //        {
        //            emailConfig.AccessToken = newAccessToken;
        //            emailConfig.AccessExpirationtime = DateTime.Now.AddHours(1);

        //            await emailConfigurationService.Update(emailConfig); 
        //        }
        //    }

        //    return emailConfig;
        //}


        public  async Task<IResponseVm<string>> WelcomeEmailWithOauth(string Email, string content)
        {
            var emailConfig = await ValidateAndRefreshEmailOauthAccessToken();
            if (emailConfig == null || emailConfig?.Data == null)
            {
                return new ResponseVm<string>
                {
                    IsSuccess = false,
                    Message = "Email Configuration not found. Please add configurations."
                };
            }

            var config = emailConfig?.Data;

            OAuthEmailRequest vm = new OAuthEmailRequest
            {
                FromEmail = config.From ?? "",
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



    public class EmailRequest
    {
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class GoogleToken
    {
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    public class GoogleTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token_expires_in")]
        public int RefreshTokenExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }



}
