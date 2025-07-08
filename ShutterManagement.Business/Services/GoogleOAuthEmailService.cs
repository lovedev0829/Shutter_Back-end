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
        private readonly EmailTypeService emailTypeService;
        private readonly string _googleOAuthGrantTypeAuthorization = "authorization_code";
        private readonly string _googleOAuthGrantTypeRefresh = "refresh_token";
        private readonly string _googleTokenInfoUrl = "https://www.googleapis.com/oauth2/v1/tokeninfo?access_token=";
        private readonly string _googleTokenUrl = "https://oauth2.googleapis.com/token";

        public GoogleAuthService(IConfiguration config,IEmailConfigurationService emailConfigurationService,
            ClaimsProperty claimsProperty,EmailTypeService emailTypeService)
        {
            _config = config;
            this.emailConfigurationService = emailConfigurationService;
            this.claimsProperty = claimsProperty;
            this.emailTypeService = emailTypeService;
        }

        public string GetAuthUrl(string userEmail)
        {
            var query = new Dictionary<string, string>
            {
                ["client_id"] = AppConstant.Google_ClientId,
                ["redirect_uri"] = AppConstant.Google_RedirectUri,
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
                var existingEmailConfiguration = (await emailConfigurationService
                    .GetAll())
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId());
                if (string.IsNullOrEmpty(existingEmailConfiguration?.Username))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "User email is required Enter Username in configuration ";
                    return responseVm;
                }
                var clientId = AppConstant.Google_ClientId;
                var redirectUri = AppConstant.Google_RedirectUri;

                var scopes = new[]
                {
                "https://www.googleapis.com/auth/gmail.send",
                "https://www.googleapis.com/auth/gmail.labels",
                "https://mail.google.com/"
                };

                var scope = string.Join(" ", scopes);

                var authUrl = $"{AppConstant.Google_AuthUrl}" +
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
                    ["client_id"] = AppConstant.Google_ClientId,
                    ["client_secret"] = AppConstant.Google_ClientSecret,
                    ["redirect_uri"] = AppConstant.Google_RedirectUri,
                    ["grant_type"] = _googleOAuthGrantTypeAuthorization
                };

                using var client = new HttpClient();
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(_googleTokenUrl, content);

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

                var tokenResponse = JsonConvert.DeserializeObject<OAuthRefereshTokenResponse>(result);

                var config = emailConfigurationService
                    .GetAllQuerable()
                    .FirstOrDefault(x => x.TenantId == claimsProperty.GetTenantId());

                if (config != null)
                {
                    config.AccessToken = tokenResponse?.access_token;
                    config.RefreshToken = tokenResponse?.refresh_token;
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
                ["client_id"] =  AppConstant.Google_ClientId,
                ["client_secret"] = AppConstant.Google_ClientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = _googleOAuthGrantTypeRefresh
            };

            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(_googleTokenUrl, content);
            var result = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(result);
            if(json != null && !string.IsNullOrEmpty(json.access_token)) 
            return json.access_token;

            return string.Empty;
        }

        public async Task<bool> IsGoogleAccessTokenValid(string accessToken)
        {
            using var client = new HttpClient();

            try
            {
                var response = await client.GetAsync($"{_googleTokenInfoUrl}{accessToken}");

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



        public async Task<IResponseVm<EmailConfigurationVm?>> ValidateAndRefreshGoogleEmailOauthAccessToken(TblEmailConfiguration emailConfig, string EmailType)
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

                isAccessTokenValid = await IsGoogleAccessTokenValid(emailConfig.AccessToken ?? "");

                if (!isAccessTokenValid || isAccessTokenExpired)
                {
                    var newAccessToken = await GoogleRefreshAccessToken(emailConfig.RefreshToken ?? "");

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
