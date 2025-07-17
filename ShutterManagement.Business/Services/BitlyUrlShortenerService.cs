using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Response;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class BitlyUrlShortenerService : IUrlShortenerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _bitlyApiToken;
        private const string BITLY_API_BASE_URL = "https://api-ssl.bitly.com/v4";

        public BitlyUrlShortenerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _bitlyApiToken = AppConstant.BitlyApiToken;
        }

        public async Task<IResponseVm<string>> ShortenUrlAsync(string longUrl)
        {
            var responseVm = new ResponseVm<string>();

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(longUrl))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "URL cannot be empty";
                    responseVm.Data = longUrl;
                    FileLogger.Log("BitlyUrlShortenerService: Empty URL provided for shortening");
                    return responseVm;
                }

                // Validate Bitly token
                if (string.IsNullOrWhiteSpace(_bitlyApiToken))
                {
                    responseVm.IsSuccess = false;
                    responseVm.Message = "Bitly API token not configured";
                    responseVm.Data = longUrl; // Return original URL as fallback
                    FileLogger.Log("BitlyUrlShortenerService: Bitly API token not configured");
                    return responseVm;
                }

                // Prepare the request payload
                var requestPayload = new
                {
                    long_url = longUrl,
                    domain = !string.IsNullOrWhiteSpace(AppConstant.BitlyDomain) ? AppConstant.BitlyDomain : "bit.ly" // Use custom domain if configured, fallback to bit.ly
                };

                var jsonContent = JsonSerializer.Serialize(requestPayload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Set up the HTTP request
                var request = new HttpRequestMessage(HttpMethod.Post, $"{BITLY_API_BASE_URL}/shorten")
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Bearer {_bitlyApiToken}");

                // Send the request
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parse the successful response
                    var bitlyResponse = JsonSerializer.Deserialize<BitlyResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (bitlyResponse?.Link != null)
                    {
                        responseVm.IsSuccess = true;
                        responseVm.Data = bitlyResponse.Link;
                        responseVm.Message = "URL shortened successfully";
                        
                        FileLogger.Log($"BitlyUrlShortenerService: Successfully shortened URL from {longUrl} to {bitlyResponse.Link}");
                        return responseVm;
                    }
                }

                // Handle API errors
                var errorMessage = $"Bitly API error: {response.StatusCode} - {responseContent}";
                FileLogger.Log($"BitlyUrlShortenerService: {errorMessage}");

                // Return original URL as fallback
                responseVm.IsSuccess = false;
                responseVm.Message = "URL shortening failed, using original URL";
                responseVm.Data = longUrl;
                
                return responseVm;
            }
            catch (HttpRequestException httpEx)
            {
                FileLogger.Log($"BitlyUrlShortenerService HTTP error: {httpEx.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = "Network error during URL shortening";
                responseVm.Data = longUrl; // Return original URL as fallback
                return responseVm;
            }
            catch (TaskCanceledException tcEx)
            {
                FileLogger.Log($"BitlyUrlShortenerService timeout: {tcEx.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = "URL shortening request timed out";
                responseVm.Data = longUrl; // Return original URL as fallback
                return responseVm;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"BitlyUrlShortenerService unexpected error: {ex.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = "Unexpected error during URL shortening";
                responseVm.Data = longUrl; // Return original URL as fallback
                return responseVm;
            }
        }

        public async Task<IResponseVm<object>> GetUrlAnalyticsAsync(string shortenedUrl)
        {
            var responseVm = new ResponseVm<object>();

            try
            {
                // This is a placeholder for future analytics functionality
                // Bitly provides analytics endpoints that can be implemented later
                responseVm.IsSuccess = false;
                responseVm.Message = "Analytics feature not yet implemented";
                
                await Task.CompletedTask; // Remove this when implementing actual analytics
                
                return responseVm;
            }
            catch (Exception ex)
            {
                FileLogger.Log($"BitlyUrlShortenerService analytics error: {ex.Message}");
                responseVm.IsSuccess = false;
                responseVm.Message = ex.Message;
                return responseVm;
            }
        }

        // Internal class for deserializing Bitly API responses
        private class BitlyResponse
        {
            public string Link { get; set; } = string.Empty;
            public string Id { get; set; } = string.Empty;
            public string LongUrl { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
} 