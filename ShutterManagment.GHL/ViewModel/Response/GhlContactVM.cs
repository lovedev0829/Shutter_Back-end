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

            
        public string id { get; set; }

        public string locationId { get; set; }

        public string fullNameLowerCase { get; set; }

        public string firstName { get; set; }

        public string lastName { get; set; }


        public string email { get; set; }

        public string phone { get; set; }

     
        public string type { get; set; }


        public DateTime dateAdded { get; set; }
        public DateTime dateUpdated { get; set; }

    }
    public class GhlContactResponse
    {
        [JsonPropertyName("contacts")]
        public List<GhlContactVM> Contacts { get; set; }
    }

    public class GhlContactSingleResponse
    {
        public GhlContactVM Contact { get; set; }
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

        [JsonProperty("locationId")]
        public string locationId { get; set; }
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
