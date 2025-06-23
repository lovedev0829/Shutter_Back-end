using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagment.GHL.ViewModel.Response
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class CreatedBy
        {
            public string userId { get; set; }
            public string source { get; set; }
        }

        public class GHLAppointment
        {
        public string appointmentStatus { get; set; }
        public string address { get; set; }
        public string calendarId { get; set; }
        public string contactId { get; set; }
        public DateTime dateAdded { get; set; }
        public DateTime dateUpdated { get; set; }
        public DateTime endTime { get; set; }
        public string id { get; set; }
        public string locationId { get; set; }
        public DateTime startTime { get; set; }
        public string title { get; set; }
        public List<object> assignedResources { get; set; }
        public bool isRecurring { get; set; }
        public string rrule { get; set; }
        public CreatedBy createdBy { get; set; }
    }

        public class GHLAppointmentResponse
        {
        public GHLAppointment appointment { get; set; }
        public string traceId { get; set; }
    }


    }

