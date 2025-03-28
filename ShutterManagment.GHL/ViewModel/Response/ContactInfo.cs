using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShutterManagment.GHL.ViewModel.Response
{
    public class ContactInfo
    {
        [JsonProperty("id")]
        public string ContactId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
    }

}
