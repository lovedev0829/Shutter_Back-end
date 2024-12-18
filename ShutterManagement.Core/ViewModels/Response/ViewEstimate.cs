using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class ViewEstimate
    {
        public Guid? EstimateId { get; set; }

        public string? Address { get; set; }
        public string? AddressLabel { get; set; }
        public string? JobName { get; set; }
        public string? JobType { get; set; }
        public string? AssignedPersonnel { get; set; }
        public string? JobNotes { get; set; }
        public DateTime? ScheduleDate_Time { get; set; }

        public List<WindowDetails>? WindowDetail { get; set; }

        public class WindowDetails
        {
            public string? WindowLabel { get; set; }
            public string? ProductType { get; set; }
            public decimal? Width { get; set; }
            public decimal? Height { get; set; }
            public string? Options { get; set; }
            public decimal? Price { get; set; }
        }



    }
}
