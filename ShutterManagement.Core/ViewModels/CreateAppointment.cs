using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class CreateAppointment
    {
        [Required]
        public Guid? JobId { get; set; }

        [Required]
        public DateTime? ScheduleDateTime { get; set; }

        [Required]
        public string? Title { get; set; }
        public string? GhlUserId { get; set; }
        public Guid? UserId { get; set; }
        public string? Description { get; set; }

    }
}
