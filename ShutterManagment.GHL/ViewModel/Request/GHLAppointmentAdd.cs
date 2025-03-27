using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagment.GHL.ViewModel.Request
{
    public class GHLAppointmentAdd
    {
        public string locationId { get; set; }
        public string startTime { get; set; }
       // public string calendarId { get; set; }
        public string contactId { get; set; }
        public string endtime { get; set; }
    }
}
