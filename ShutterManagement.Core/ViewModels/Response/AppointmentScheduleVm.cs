using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class AppointmentScheduleVm
    {
        public Guid? JobId { get; set; }
        public Guid? UserId { get; set; }

        public DateTime? ScheduleDateTime { get; set; }

        public bool? IsActive { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }

        public string? GhlUserId { get; set; }
        public string? GhlUserName { get; set; }


    }
}
