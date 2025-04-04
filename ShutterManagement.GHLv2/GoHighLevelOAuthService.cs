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
            { "client_id", configuration["GoHighLevelOAuthSettings:ClientId"] ?? "" },
            { "redirect_uri",  configuration["GoHighLevelOAuthSettings:RedirectUri"] ?? ""},
            { "response_type", "code" },
            { "scope", "conversations/message.readonly conversations/message.write contacts.readonly contacts.write opportunities.readonly opportunities.write calendars.readonly " +
            "calendars.write calendars/events.readonly calendars/events.write calendars/groups.readonly calendars/groups.write calendars/resources.readonly calendars/resources.write users.readonly" } 
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{goHighLevelOAuthSettings.AuthorizationUrl}?{queryString}";
    }

    // Step 2: Exchange the authorization code for an access token
    public async Task<TokenResponse> GetRefreshToken(string authorizationCode)
    {
        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "client_id", configuration["GoHighLevelOAuthSettings:ClientId"] ?? ""},
        { "client_secret", configuration["GoHighLevelOAuthSettings:ClientSecret"] ?? ""},
        { "code", authorizationCode },
        { "redirect_uri", configuration["GoHighLevelOAuthSettings:RedirectUri"] ?? ""},
        { "grant_type", "authorization_code" }
    });

        var response = await _httpClient.PostAsync(goHighLevelOAuthSettings.TokenUrl, tokenRequest);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
            return tokenData;
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return null;
        }
    }
    // Step 3: Exchange the refreshtoken for an access token
    public async Task<TokenResponse> GetAccessToken(string refreshtoken)
    {
        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        { "client_id", AppConstant.GHL_ClientId },
        { "client_secret", AppConstant.GHL_ClientSecret },
        { "refresh_token", refreshtoken },
        { "redirect_uri", AppConstant.GHL_RedirectUri },
        { "grant_type", "refresh_token" }
    });

        var response = await _httpClient.PostAsync(goHighLevelOAuthSettings.TokenUrl, tokenRequest);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
            return tokenData;
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error retrieving access token: {errorContent}");
            return null;
        }
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