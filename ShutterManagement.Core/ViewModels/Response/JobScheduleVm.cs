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
        public string? JobTitle { get; set; }
        public Guid? UserId { get; set; }
        public Guid? JobId { get; set; }
        public string? UserFullname { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeOnly? DayStartTime { get; set; }
        public TimeOnly? DayEndDateTime { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? EstimateId { get; set; }
        public string? EstimateName { get; set; }
        public bool? IsActive { get; set; }
    }
}

