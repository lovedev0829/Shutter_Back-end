using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateJobSchedule :CreateJobSchedule
    {
        public Guid? ScheduleJobId { get; set; }
    }
}
