using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateJobSchedule
    {

        [Required(ErrorMessage = "JobTitleId is required.")]
        public Guid? JobTitleId { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public Guid? UserId { get; set; }
        
        [Required(ErrorMessage = "JobId is required.")]
        public Guid? JobId { get; set; }

        [Required(ErrorMessage = "StartDate is required.")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required.")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "DayStartTime is required.")]
        public string? DayStartTime { get; set; }

        [Required(ErrorMessage = "DayEndDateTime is required.")]
        public string? DayEndDateTime { get; set; }


    }
}

