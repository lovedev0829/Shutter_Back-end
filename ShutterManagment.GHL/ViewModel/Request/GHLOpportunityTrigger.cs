using Newtonsoft.Json;
using ShutterManagment.GHL.ViewModel.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShutterManagment.GHL.ViewModel.Request
{
    public class GHLOpportunityTrigger : GHLContactTrigger
    {
        
        public string? opportunity_name { get; set; }
        public string? status { get; set; }
        public string? pipleline_stage { get; set; }
        public string? pipeline_id { get; set; }
        public string? id { get; set; }
        public string? pipeline_name { get; set; }

        [JsonPropertyName("user")]
        public User? Users { get; set; } = new User();
    }

    //public partial class User
    //{
    //    [JsonProperty("firstName")]
    //    public string FirstName { get; set; }

    //    [JsonProperty("lastName")]
    //    public string LastName { get; set; }

    //    [JsonProperty("email")]
    //    public string Email { get; set; }
    //}


}
