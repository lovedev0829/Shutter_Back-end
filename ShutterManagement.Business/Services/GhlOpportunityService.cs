using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShutterManagement.Core;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Business.Services
{
    public class GhlOpportunityService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://rest.gohighlevel.com/v1/opportunities"; // Replace with the actual API endpoint
        private readonly string _apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2NhdGlvbl9pZCI6IlBUbklDTU4wbUtyMDNrekpRcVBRIiwidmVyc2lvbiI6MSwiaWF0IjoxNzI0MDcwNDM3MTYzLCJzdWIiOiJkOGhlWkl2dEliZDVPcGhGdXNmYiJ9.yRrmWRgUC5VtrhKMsE4JsLjpwROYvvQnwoYu5JbVQWU"; // Replace with your API key

        public GhlOpportunityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> CreateOpportunityFromEstimateAsync(CreateEstimate estimate)
        {
            var opportunityData = new
            {
                name = estimate.EstimateName, // Assuming "Name" is part of your estimate model
                status = "New", // Default status for new opportunities
                stage = "Lead", // Opportunity stage
               // owner_id = AppConstant.LoggedInUserID, // Map this to your system's user ID
                // Add other necessary fields here
            };

            var content = new StringContent(JsonConvert.SerializeObject(opportunityData), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync(_apiUrl, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UpdateOpportunityFromEstimateAsync(UpdatetEstimate estimate)
        {
            var opportunityData = new
            {
                name = estimate.EstimateName,
               // status = estimate.,  // Assuming this is a part of your estimate
                                           // Map other fields accordingly
            };

            var content = new StringContent(JsonConvert.SerializeObject(opportunityData), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PutAsync($"{_apiUrl}/{estimate.EstimateId}", content);  // Use the correct OpportunityId here
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

    }
}
