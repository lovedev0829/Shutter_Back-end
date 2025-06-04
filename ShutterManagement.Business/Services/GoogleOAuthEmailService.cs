using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Newtonsoft.Json;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class GoogleAuthService
    {
        private readonly IConfiguration _config;
        private readonly IEmailConfigurationService emailConfigurationService;
        private readonly ClaimsProperty claimsProperty;
        private readonly ITenantService tenant;

        public GoogleAuthService(IConfiguration config,IEmailConfigurationService emailConfigurationService,
            ClaimsProperty claimsProperty,ITenantService tenant)
        {
            _config = config;
            this.emailConfigurationService = emailConfigurationService;
            this.claimsProperty = claimsProperty;
            this.tenant = tenant;
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
                    .FirstOrDefault(x => x.TenantId == AppConstant.LoggedInTenantId);
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

                var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(result);

                var config = emailConfigurationService
                    .GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == AppConstant.LoggedInTenantId);

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


        public async Task<string> RefreshAccessTokenAsync(string refreshToken)
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
