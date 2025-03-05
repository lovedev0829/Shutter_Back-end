using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagment.GHL.ViewModel.Response
{
    public class GhlOpportunityWebhookModel
    {
        [JsonProperty("eventType")]
        public string EventType { get; set; }  

        [JsonProperty("locationId")]
        public string LocationId { get; set; }  

        [JsonProperty("opportunity")]
        public GhlOpportunityData Opportunity { get; set; }
    }

    public class GhlOpportunityData
    {
        [JsonProperty("id")]
        public string Id { get; set; }  

        [JsonProperty("title")]
        public string Title { get; set; }  

        [JsonProperty("status")]
        public string Status { get; set; }  

        [JsonProperty("stageId")]
        public string StageId { get; set; }  

        [JsonProperty("pipelineId")]
        public string PipelineId { get; set; }  

        [JsonProperty("monetaryValue")]
        public decimal? MonetaryValue { get; set; }  

        [JsonProperty("description")]
        public string Description { get; set; }  

        [JsonProperty("contact")]
        public GhlContactData Contact { get; set; }  
    }

    public class GhlContactData
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
    }

}
