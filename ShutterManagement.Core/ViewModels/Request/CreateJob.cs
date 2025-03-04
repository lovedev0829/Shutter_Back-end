using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateJob
    {
        [DefaultValue("")]
        public Guid? ContactId { get; set; }

        [DefaultValue("")]
        [Required]
        public Guid? AddressId { get; set; }

        [DefaultValue("")]
        [MaxLength(200)]
        [Required]
        public string? JobName { get; set; }

        [DefaultValue("")]
        [Required]
        public Guid? JobType { get; set; }

        [DefaultValue("")]
        [Required]
        public Guid? AssignedPersonnel { get; set; }

        [DefaultValue("")]
        [Required]
        public DateTime? ScheduledDate { get; set; }

        //[DefaultValue("")]
        //[Required]
        //public string? JobStatus { get; set; }

        [DefaultValue("")]
      
        public string? Notes { get; set; }
        public Guid? GHL_StageId { get; set; }
       // public string? GHL_StageName { get; set; }
        public string[]? AttachmentsAny { get; set; } = [];

    }
}
