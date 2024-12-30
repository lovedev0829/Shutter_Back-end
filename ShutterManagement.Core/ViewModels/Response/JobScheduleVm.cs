using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class JobScheduleVm
    {
        public Guid? ScheduleJobId { get; set; }
        public Guid? JobTitleId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeOnly? DayStartTime { get; set; }
        public TimeOnly? DayEndDateTime { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }
    }
}

