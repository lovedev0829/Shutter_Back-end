using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShutterManagment.GHL.ViewModel.Request
{

    public class AttributionSource
    {
        public string? medium { get; set; }
        public string? mediumId { get; set; }
    }

    public class Contact
    {
        public AttributionSource? attributionSource { get; set; }
        public LastAttributionSource? lastAttributionSource { get; set; }
    }

    public class CustomData
    {
    }

    public class LastAttributionSource
    {
    }

    public class Location
    {
        public string? name { get; set; }
        public string? address { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? country { get; set; }
        public string? postalCode { get; set; }
        public string? fullAddress { get; set; }
        public string id { get; set; }
    }

    public class GHLContactTrigger
    {
        public string contact_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string? full_name { get; set; }
        public string email { get; set; }
        public string? phone { get; set; }
        public string? tags { get; set; }
        public DateTime? date_created { get; set; }
        public string? company_name { get; set; }
        public string? full_address { get; set; }
        public string contact_type { get; set; }
        public string? WebhookTenantId { get; set; }

        [JsonPropertyName("Street Address")]
        public string? StreetAddress { get; set; }
        public string? address1 { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? postal_code { get; set; }
        public Location? location { get; set; }
        public Workflow? workflow { get; set; }
        public TriggerData? triggerData { get; set; }
        public Contact? contact { get; set; }
        public AttributionSource? attributionSource { get; set; }
        public CustomData? customData { get; set; }
    }

    public class TriggerData
    {
    }

    public class Workflow
    {
        public string? id { get; set; }
        public string? name { get; set; }
    }



}
