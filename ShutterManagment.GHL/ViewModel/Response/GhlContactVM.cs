using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShutterManagment.GHL.ViewModel.Response
{
    public class GhlContactVM
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("locationId")]
        public string LocationId { get; set; }

        [JsonPropertyName("contactName")]
        public string ContactName { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("dnd")]
        public bool Dnd { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("assignedTo")]
        public string AssignedTo { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonPropertyName("address1")]
        public string Address1 { get; set; }

        [JsonPropertyName("dateAdded")]
        public DateTime DateAdded { get; set; }

        [JsonPropertyName("dateUpdated")]
        public DateTime DateUpdated { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("website")]
        public string Website { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("JobType")]
        public string JobType { get; set; }

        [JsonPropertyName("lastActivity")]
        public long LastActivity { get; set; }

        [JsonPropertyName("customField")]
        public List<object> CustomField { get; set; }
    }
    public class GhlContactResponse
    {
        [JsonPropertyName("contacts")]
        public List<GhlContactVM> Contacts { get; set; }
    }

    public class GhlOpportunitiesResponse
    {
        [JsonProperty("opportunities")]
        public List<Opportunity> Opportunities { get; set; }

        [JsonProperty("meta")]
        public MetaData Meta { get; set; }
    }

    public class Opportunity
    {
        [JsonProperty("id")]
        public string OpportunityId { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("monetaryValue")]
        public decimal MonetaryValue { get; set; }

        [JsonProperty("pipelineId")]
        public string? PipelineId { get; set; }

        [JsonProperty("pipelineStageId")]
        public string? PipelineStageId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("contact")]
        public Contact Contact { get; set; }
    }

    public class Contact
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
    }

    public class MetaData
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("nextPageUrl")]
        public string NextPageUrl { get; set; }

        [JsonProperty("startAfterId")]
        public string StartAfterId { get; set; }
    }

}
