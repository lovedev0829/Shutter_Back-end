using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


public class GoHighLevelOAuthService
{
    private const string AuthorizationUrl = "https://marketplace.gohighlevel.com/oauth/authorize";
    private const string TokenUrl = "https://auth.gohighlevel.com/oauth/token";
    private const string RedirectUri = "https://localhost:44355/common/GenerateToken"; 
    private const string ClientId = "67e11c72c90a2f81200cc12a-m8mtsqrn";
    private const string ClientSecret = "c23af409-223c-48c1-a0d5-b1f25bbc2336";

    private readonly HttpClient _httpClient;

    public GoHighLevelOAuthService()
    {
        _httpClient = new HttpClient();
    }

    // Step 1: Get the Authorization URL for the user to log in
    public string GetAuthorizationUrl()
    {
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", ClientId },
            { "client_secret", ClientSecret },
            { "redirect_uri", RedirectUri },
            { "response_type", "code" },
            { "scope", "read write" } 
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{AuthorizationUrl}?{queryString}";
    }

    // Step 2: Exchange the authorization code for an access token
    public async Task<string> GetAccessToken(string authorizationCode)
    {
        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", ClientId },              
            { "client_secret", ClientSecret },       
            { "code", authorizationCode },          
            { "redirect_uri", RedirectUri },        
            { "grant_type", "authorization_code" }  
        });

        var response = await _httpClient.PostAsync(TokenUrl, tokenRequest);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<TokenResponse>(responseContent);
            return tokenData.AccessToken; 
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error retrieving access token: {errorContent}");
        }
    }

    private class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
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