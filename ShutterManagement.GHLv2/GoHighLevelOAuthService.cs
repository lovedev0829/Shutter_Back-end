using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShutterManagement.GHLv2;
using Microsoft.Extensions.Options;
using ShutterManagement.Core;
using Microsoft.Extensions.Configuration;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using Azure;
using static System.Net.WebRequestMethods;


public class GoHighLevelOAuthService
{
    //private const string AuthorizationUrl = "https://marketplace.gohighlevel.com/oauth/chooselocation";
    //private const string TokenUrl = "https://services.leadconnectorhq.com/oauth/token";
    //private const string RedirectUri = "http://192.168.0.22:5500/configuration"; 
    //private const string ClientId = "67e11c72c90a2f81200cc12a-m8mtsqrn";
    //private const string ClientSecret = "c23af409-223c-48c1-a0d5-b1f25bbc2336";

    private readonly HttpClient _httpClient;
    private readonly GoHighLevelOAuthSettings goHighLevelOAuthSettings;
    private readonly IConfiguration configuration;

    public GoHighLevelOAuthService(IOptions<GoHighLevelOAuthSettings> goHighLevelOAuthSettings, IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        this.goHighLevelOAuthSettings = goHighLevelOAuthSettings.Value;
        this.configuration = configuration;
    }

    //ClientId = configuration["GoHighLevelOAuthSettings:ClientId"] ?? "",
    //                    ClientSecret = configuration["GoHighLevelOAuthSettings:ClientSecret"] ?? "",
    // Step 1: Get the Authorization URL for the user to log in
    public string GetAuthorizationUrl()
    {
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", AppConstant.GHL_Marketplace_ClientId ?? "" },
            { "redirect_uri",  AppConstant.GHL_Marketplace_RedirectUri ?? ""},
            { "response_type", "code" },
            { "scope", "conversations/message.readonly conversations/message.write contacts.readonly contacts.write opportunities.readonly opportunities.write calendars.readonly " +
            "calendars.write calendars/events.readonly calendars/events.write calendars/groups.readonly calendars/groups.write calendars/resources.readonly calendars/resources.write users.readonly" } 
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{configuration["GoHighLevelOAuthSettings:AuthorizationUrl"] ?? ""}?{queryString}";
    }

    // Step 2: Exchange the authorization code for an access token

    public async Task<IResponseVm<TokenResponse>> GetRefreshToken(string authorizationCode)
    {
        var responseVm = new ResponseVm<TokenResponse>();

        try
        {
            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", AppConstant.GHL_Marketplace_ClientId ?? "" },
            { "client_secret", AppConstant.GHL_Marketplace_ClientSecret ?? "" },
            { "code", authorizationCode },
            { "redirect_uri", AppConstant.GHL_Marketplace_RedirectUri ?? "" },
            { "grant_type", "authorization_code" }
        });

            var response = await _httpClient.PostAsync(AppConstant.GHL_Marketplace_TokenURL ?? "", tokenRequest);

            FileLogger.Log($"This is the Logs of Refresh token response: {response}");

            var responseContent = await response.Content.ReadAsStringAsync();
            FileLogger.Log($"This is the Logs of Refresh token content: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var tokenData = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                    if (tokenData != null)
                    {
                        responseVm.IsSuccess = true;
                        responseVm.Message = "Token retrieved successfully.";
                        responseVm.Data = tokenData;
                        FileLogger.Log($"Token retrieved successfully.: {tokenData}");
                    }
                    else
                    {
                        responseVm.IsSuccess = false;
                        responseVm.Message = "Token response is null. Could not deserialize properly.";
                        FileLogger.Log($"Token response is null. Could not deserialize properly");
                    }
                }
                catch (JsonException jsonEx)
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = $"Error deserializing the token response: {jsonEx.Message}";
                    FileLogger.Log($"Error deserializing the token response: {jsonEx.Message}");
                }
            }
            else
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Failed to get token. StatusCode: {response.StatusCode}, Error: {responseContent}";
            }
        }
        catch (HttpRequestException httpEx)
        {
            responseVm.IsSuccess = false;
            responseVm.Message = $"Network error while requesting token: {httpEx.Message}";
            FileLogger.Log($"Network error while requesting token: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            responseVm.IsSuccess = false;
            responseVm.Message = $"An error occurred while retrieving the refresh token: {ex.Message}";
        }

        return responseVm;
    }



    //public async Task<TokenResponse> GetRefreshToken(string authorizationCode)
    //{
    //    try
    //    {
    //        // Create the form data for the token request
    //        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
    //    {
    //        { "client_id", configuration["GoHighLevelOAuthSettings:ClientId"] ?? "" },
    //        { "client_secret", configuration["GoHighLevelOAuthSettings:ClientSecret"] ?? "" },
    //        { "code", authorizationCode },
    //        { "redirect_uri", configuration["GoHighLevelOAuthSettings:RedirectUri"] ?? "" },
    //        { "grant_type", "authorization_code" }
    //    });

    //        // Perform the POST request to the token URL
    //        var response = await _httpClient.PostAsync(configuration["GoHighLevelOAuthSettings:TokenUrl"] ?? "", tokenRequest);

    //        FileLogger.Log($"This is the Logs of Refesh token {response}");
    //        // Check if the response is successful
    //        if (response.IsSuccessStatusCode)
    //        {
    //            var responseContent = await response.Content.ReadAsStringAsync();
    //            FileLogger.Log($"This is the Logs of Refesh token {response}");
    //            try
    //            {
    //                // Try deserializing the response content
    //                var tokenData = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
    //                if (tokenData == null)
    //                {
    //                    throw new Exception("Token response is null. The response content could not be deserialized properly.");
    //                }

    //                return tokenData;
    //            }
    //            catch (JsonException jsonEx)
    //            {
    //                // Log the deserialization error
    //                //logger.LogError(jsonEx, "Error deserializing the token response content.");
    //                throw new Exception("Error deserializing the token response.", jsonEx);
    //            }
    //        }
    //        else
    //        {
    //            // If the response is not successful, log the error and return null
    //            var errorContent = await response.Content.ReadAsStringAsync();
    //            // Optionally log the error response content for debugging
    //            //logger.LogError($"Failed to get refresh token. StatusCode: {response.StatusCode}, Error: {errorContent}");
    //            return null;
    //        }
    //    }
    //    catch (HttpRequestException httpEx)
    //    {
    //        // Handle network-related exceptions (e.g., no internet connection)
    //        //logger.LogError(httpEx, "Network error while calling the GoHighLevel OAuth service.");
    //        throw new Exception("A network error occurred while requesting the token.", httpEx);
    //    }
    //    catch (Exception ex)
    //    {
    //        // Catch all other exceptions
    //        //logger.LogError(ex, "An error occurred while getting the refresh token.");
    //        throw new Exception("An error occurred while retrieving the refresh token.", ex);
    //    }
    //}

    // Step 3: Exchange the refreshtoken for an access token
    public async Task<IResponseVm<TokenResponse>> GetAccessToken(string refreshtoken)
    {
        var responseVm = new ResponseVm<TokenResponse>();

        try
        {
            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", AppConstant.GHL_Marketplace_ClientId ?? "" },
            { "client_secret", AppConstant.GHL_Marketplace_ClientSecret ?? "" },
            { "refresh_token", refreshtoken },
            { "redirect_uri", AppConstant.GHL_Marketplace_RedirectUri ?? "" },
            { "grant_type", "refresh_token" }
        });

            var response = await _httpClient.PostAsync(AppConstant.GHL_Marketplace_TokenURL ?? "", tokenRequest);

            FileLogger.Log($"Access token response: {response}");

            var responseContent = await response.Content.ReadAsStringAsync();
            FileLogger.Log($"Access token content: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var tokenData = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                if (tokenData != null)
                {
                    responseVm.IsSuccess = true;
                    responseVm.Message = "Access token retrieved successfully.";
                    responseVm.Data = tokenData;
                    FileLogger.Log($"Access token retrieved successfully.: {tokenData}");
                }
                else
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Failed to deserialize access token response.";
                }
            }
            else
            {
                responseVm.IsSuccess = false;
                responseVm.Message = $"Failed to get access token. StatusCode: {response.StatusCode}, Error: {responseContent}";
                FileLogger.Log($"Failed to get access token. StatusCode: {response.StatusCode}, Error: {responseContent}");
            }
        }
        catch (HttpRequestException httpEx)
        {
            responseVm.IsSuccess = false;
            responseVm.Message = $"Network error while requesting access token: {httpEx.Message}";
            FileLogger.Log($"Network error while requesting access token: {httpEx.Message}");
        }
        catch (JsonException jsonEx)
        {
            responseVm.IsSuccess = false;
            responseVm.Message = $"Error deserializing access token response: {jsonEx.Message}";
            FileLogger.Log($"Error deserializing access token response: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            responseVm.IsSuccess = false;
            responseVm.Message = $"Unexpected error occurred while retrieving access token: {ex.Message}";
            FileLogger.Log($"Unexpected error occurred while retrieving access token: {ex.Message}");
        }

        return responseVm;
    }



    //public class GoHighLevelApiService
    //{
    //    private readonly string _accessToken;
    //    private readonly string _baseUrl = "https://api.gohighlevel.com/v1/";
    //    private readonly HttpClient _httpClient;

    //    public GoHighLevelApiService(string accessToken)
    //    {
    //        _accessToken = accessToken;
    //        _httpClient = new HttpClient();
    //        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
    //        _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
    //    }

    //    // Example: Create a new contact
    //    public async Task CreateContact(string firstName, string lastName, string email, string phone)
    //    {
    //        var contact = new
    //        {
    //            first_name = firstName,
    //            last_name = lastName,
    //            email = email,
    //            phone = phone,
    //            tags = new[] { "New Lead" }
    //        };

    //        var jsonContent = JsonConvert.SerializeObject(contact);
    //        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

    //        try
    //        {
    //            var response = await _httpClient.PostAsync($"{_baseUrl}contacts", content);
    //            if (response.IsSuccessStatusCode)
    //            {
    //                Console.WriteLine("Contact created successfully!");
    //            }
    //            else
    //            {
    //                Console.WriteLine($"Error creating contact: {response.StatusCode} - {response.ReasonPhrase}");
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"An error occurred: {ex.Message}");
    //        }
    //    }
    //}






}