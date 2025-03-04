using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
//using ShutterManagement.DAL.Context;
using ShutterManagement.GHL;


namespace ShutterManagement.GHL.Service
{
    public class GhlService
    {
        private readonly HttpClient _httpClient;
      //  private readonly ShutterDBContext dBContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private const string BaseUrl = "https://rest.gohighlevel.com/v1/";
        private const string ApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2NhdGlvbl9pZCI6IlBUbklDTU4wbUtyMDNrekpRcVBRIiwidmVyc2lvbiI6MSwiaWF0IjoxNzI0MDcwNDM3MTYzLCJzdWIiOiJkOGhlWkl2dEliZDVPcGhGdXNmYiJ9.yRrmWRgUC5VtrhKMsE4JsLjpwROYvvQnwoYu5JbVQWU"; // Replace with your actual API key

        public GhlService(HttpClient httpClient,IHttpContextAccessor _httpContextAccessor)
        {
            _httpClient = httpClient;
          //  this.dBContext = dBContext;
            httpContextAccessor = _httpContextAccessor;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ApiKey);
        }

        public async Task<List<GhlContact>> GetContactsAsync()
        {
            var response = await _httpClient.GetAsync("contacts/");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<GhlContactResponse>(json);
            var result1 = GetClientIpAddress();
            return result?.Contacts ?? new List<GhlContact>();
        }

        public async Task<bool> CreateContactAsync(GhlContactAdd contact)
        {
            try
            {
                var contactData = new
                {
                    firstName = contact.FirstName,
                    lastName = contact.LastName,
                    email = contact.Email,
                    phone = contact.Phone
                };

                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("contacts", content); 
                if(!response.IsSuccessStatusCode)
                {
                    return false;
                }
                response.EnsureSuccessStatusCode();
                var result1 = GetClientIpAddress();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var contactResponse = JsonConvert.DeserializeObject<GhlContact>(jsonResponse);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateContactAsync(string contactId, GhlContactAdd contact)
        {
            try
            {
                if (string.IsNullOrEmpty(contactId) || contact == null)
                {
                    return false;
                }

                var contactData = new
                {
                    firstName = contact.FirstName,
                    lastName = contact.LastName,
                    email = contact.Email,
                    phone = contact.Phone
                };

                var contactJson = JsonConvert.SerializeObject(contactData);
                var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"contacts/{contactId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to update contact. Status code: {response.StatusCode}");
                    return false;
                }

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var contactResponse = JsonConvert.DeserializeObject<GhlContact>(jsonResponse);

                return true; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating contact: {ex.Message}");
                return false; 
            }
        }

        public string GetClientIpAddress()
        {
            var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            return ipAddress;
        }
        //public async Task UpdateContactInGhlAsync(GhlContact contact)
        //{
        //    var url = $"{BaseUrl1}/{contact.Id}";  // Assuming `contact.Id` is the contact's unique ID
        //    var content = new StringContent(JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json");

        //    var response = await _httpClient.PutAsync(url, content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        Console.WriteLine($"Successfully updated contact: {contact.Id}");
        //    }
        //    else
        //    {
        //        var errorContent = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine($"Error updating contact {contact.Id}: {errorContent}");
        //    }
        //}
        //public async Task CreateContactInGhlAsync(GhlContactAdd newContact)
        //{
        //    var url = "https://api.gohighlevel.com/v2/contacts"; // Update with the correct endpoint for GoHighLevel API
        //    var content = new StringContent(JsonSerializer.Serialize(newContact), Encoding.UTF8, "application/json");
        //    try
        //    {
        //        var response = await _httpClient.PostAsync(url, content);

        //        // Log the response status code and headers for debugging
        //        Console.WriteLine($"Response Status Code: {response.StatusCode}");
        //        Console.WriteLine($"Response Headers: {response.Headers}");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            Console.WriteLine("Contact successfully created in GoHighLevel");
        //        }
        //        else
        //        {
        //            // Log detailed error content for debugging
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            Console.WriteLine($"Error creating contact: {errorContent}");
        //            Console.WriteLine($"Status Code: {response.StatusCode}");

        //            if (response.StatusCode == HttpStatusCode.Unauthorized)
        //            {
        //                Console.WriteLine("Authentication failed. Please check the API key.");
        //            }
        //            else if (response.StatusCode == HttpStatusCode.BadRequest)
        //            {
        //                Console.WriteLine("The request was malformed. Please check the data you're sending.");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log unexpected errors
        //        Console.WriteLine($"Exception while creating contact: {ex.Message}");
        //    }
        //}

        //        public async Task SyncContactsAsync()
        //        {
        //            // Fetch contacts from GoHighLevel
        //            var ghlContacts = await GetContactsFromGhlAsync();

        //            // Get contacts from your application database
        //            var appContacts = GetContactsFromApp();  // Your app's contact data

        //            // Two-way sync: 
        //            // 1. Push updates to GoHighLevel for contacts that are updated in your app
        //            foreach (var appContact in appContacts)
        //            {
        //                var ghlContact = ghlContacts.FirstOrDefault(c => c.Id == appContact.Id);

        //                // If contact is updated in your app but not in GHL, update GHL
        //                if (ghlContact != null && !AreContactsEqual(appContact, ghlContact))
        //                {
        //                    await UpdateContactInGhlAsync(appContact);
        //                }
        //                else if (ghlContact == null)
        //                {
        //                    // If the contact does not exist in GoHighLevel, create it
        //                    await CreateContactInGhlAsync(appContact);
        //                }
        //            }

        //            // 2. Pull updates from GoHighLevel and sync back to your app's database
        //            foreach (var ghlContact in ghlContacts)
        //            {
        //                var appContact = appContacts.FirstOrDefault(c => c.Id == ghlContact.Id);
        //                if (appContact == null)
        //                {
        //                    // If contact doesn't exist in your app, create it
        //                    CreateContactInApp(ghlContact); // Implement this function in your app
        //                }
        //                else if (!AreContactsEqual(appContact, ghlContact))
        //                {
        //                    // If contact is updated in GoHighLevel but not in your app, update your app
        //                    UpdateContactInApp(ghlContact); // Implement this function in your app
        //                }
        //            }
        //        }

        //        private bool AreContactsEqual(GhlContact appContact, GhlContact ghlContact)
        //        {
        //            // Logic to compare two contacts and see if they are identical
        //            // For example, comparing first name, last name, phone, email, etc.
        //            return appContact.FirstName == ghlContact.FirstName &&
        //                   appContact.LastName == ghlContact.LastName &&
        //                   appContact.Email == ghlContact.Email;
        //        }

        //        public async Task CreateContactInGhlAsync(GhlContactAdd newContact)
        //{
        //    var url = "https://api.gohighlevel.com/v2/contacts"; // Update with the correct endpoint for GoHighLevel API
        //    var content = new StringContent(JsonSerializer.Serialize(newContact), Encoding.UTF8, "application/json");

        //    try
        //    {
        //        var response = await _httpClient.PostAsync(url, content);

        //        // Log the response status code and headers for debugging
        //        Console.WriteLine($"Response Status Code: {response.StatusCode}");
        //        Console.WriteLine($"Response Headers: {response.Headers}");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            Console.WriteLine("Contact successfully created in GoHighLevel");
        //        }
        //        else
        //        {
        //            // Log detailed error content for debugging
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            Console.WriteLine($"Error creating contact: {errorContent}");
        //            Console.WriteLine($"Status Code: {response.StatusCode}");

        //            if (response.StatusCode == HttpStatusCode.Unauthorized)
        //            {
        //                Console.WriteLine("Authentication failed. Please check the API key.");
        //            }
        //            else if (response.StatusCode == HttpStatusCode.BadRequest)
        //            {
        //                Console.WriteLine("The request was malformed. Please check the data you're sending.");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log unexpected errors
        //        Console.WriteLine($"Exception while creating contact: {ex.Message}");
        //    }
        //}

        //public async Task CreateOrUpdateContactAsync(GhlContactAdd contact)
        //{
        //    // Make sure contact.Id is not null or empty
        //    if (string.IsNullOrEmpty(contact.Id))
        //    {
        //        Console.WriteLine("Contact ID is missing.");
        //        return;
        //    }

        //    var url = $"{BaseUrl}/{contact.Id}"; // Corrected to ensure it hits the contacts endpoint
        //    var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json");

        //    try
        //    {
        //        // Log the full URL and the content being sent
        //        Console.WriteLine($"Request URL: {url}");
        //        Console.WriteLine($"Request Body: {System.Text.Json.JsonSerializer.Serialize(contact)}");

        //        // First, check if the contact exists by making a GET request
        //        var checkResponse = await _httpClient.PostAsync(url);
        //        if (!checkResponse.IsSuccessStatusCode)
        //        {
        //            Console.WriteLine($"Contact not found (404): {contact.Id}");
        //            return;  // Stop if the contact doesn't exist
        //        }

        //        // Make the PUT request to update the contact
        //        var response = await _httpClient.PutAsync(url, content);

        //        // Log the response status code and headers for debugging
        //        Console.WriteLine($"Response Status Code: {response.StatusCode}");
        //        Console.WriteLine($"Response Headers: {response.Headers}");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            Console.WriteLine("Contact successfully synced to GoHighLevel");
        //        }
        //        else
        //        {
        //            // Log detailed error content for debugging
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            Console.WriteLine($"Error syncing contact: {errorContent}");
        //            Console.WriteLine($"Status Code: {response.StatusCode}");

        //            // Handle specific status codes here
        //            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        //            {
        //                Console.WriteLine("Authentication failed. Please check the API key.");
        //            }
        //            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        //            {
        //                Console.WriteLine("The contact was not found. Ensure the contact ID is valid.");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log unexpected errors
        //        Console.WriteLine($"Exception while syncing contact: {ex.Message}");
        //    }
        //}



        // Response Model for Contacts
        //public class GhlContactResponse
        //{
        //    [JsonPropertyName("contacts")]
        //    public List<GhlContact> Contacts { get; set; }
        //}

        //public class GhlContact
        //{
        //    [JsonPropertyName("id")]
        //    public string Id { get; set; } 

        //    [JsonPropertyName("locationId")]
        //    public string LocationId { get; set; }

        //    [JsonPropertyName("contactName")]
        //    public string ContactName { get; set; }

        //    [JsonPropertyName("firstName")]
        //    public string FirstName { get; set; }

        //    [JsonPropertyName("lastName")]
        //    public string LastName { get; set; }

        //    [JsonPropertyName("companyName")]
        //    public string CompanyName { get; set; }

        //    [JsonPropertyName("email")]
        //    public string Email { get; set; }

        //    [JsonPropertyName("phone")]
        //    public string Phone { get; set; }

        //    [JsonPropertyName("dnd")]
        //    public bool Dnd { get; set; }

        //    [JsonPropertyName("type")]
        //    public string Type { get; set; }

        //    [JsonPropertyName("source")]
        //    public string Source { get; set; } 

        //    [JsonPropertyName("assignedTo")]
        //    public string AssignedTo { get; set; } 

        //    [JsonPropertyName("city")]
        //    public string City { get; set; } 

        //    [JsonPropertyName("state")]
        //    public string State { get; set; } 

        //    [JsonPropertyName("postalCode")]
        //    public string PostalCode { get; set; }

        //    [JsonPropertyName("address1")]
        //    public string Address1 { get; set; } 

        //    [JsonPropertyName("dateAdded")]
        //    public DateTime DateAdded { get; set; }

        //    [JsonPropertyName("dateUpdated")]
        //    public DateTime DateUpdated { get; set; }

        //    [JsonPropertyName("dateOfBirth")]
        //    public DateTime? DateOfBirth { get; set; } 

        //    [JsonPropertyName("tags")]
        //    public List<string> Tags { get; set; }

        //    [JsonPropertyName("country")]
        //    public string Country { get; set; }

        //    [JsonPropertyName("website")]
        //    public string Website { get; set; } 

        //    [JsonPropertyName("timezone")]
        //    public string Timezone { get; set; }

        //    [JsonPropertyName("lastActivity")]
        //    public long LastActivity { get; set; }

        //    [JsonPropertyName("customField")]
        //    public List<object> CustomField { get; set; } 
        //}
    }
}
