using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShutterManagment.GHL.ViewModel.Response
{
    public class OpportunityResponse
    {
        [JsonProperty("id")]
        public string OpportunityId { get; set; }

        [JsonProperty("contact")]
        public ContactInfo Contact { get; set; }
    }
}
