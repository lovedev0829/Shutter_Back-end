using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllJobsdetails
    {
        public Guid? JobId { get; set; }
        public Guid? ContactId { get; set; }

        public Guid? AddressId { get; set; }

        public string? JobName { get; set; }
        public Guid? JobType { get; set; }

        public Guid? AssignedPersonnel { get; set; }

        public DateTime? ScheduledDate { get; set; }

   
        public Guid? JobStatus { get; set; }

        public string? Notes { get; set; }
        public string? AttachmentsAny { get; set; }
        public bool? IsActive { get; set; }

        public string? Label { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
      
    }
}
