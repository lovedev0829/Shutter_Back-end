using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllJobsSubtenant_tenant
    {
        public Guid? JobId { get; set; }
        public Guid? ContactId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        public Guid? AddressId { get; set; }

        public string? JobName { get; set; }
        public Guid? JobType { get; set; }
        public string? JobTypeName { get; set; }

        public Guid? AssignedPersonnel { get; set; }
        public string? AssignedPersonnelName { get; set; }

        public DateTime? ScheduledDate { get; set; }


        public Guid? JobStatus { get; set; }
        public string? JobStatusName { get; set; }

        public string? Notes { get; set; }
        public string? AttachmentsAny { get; set; }
        public bool? IsActive { get; set; }

        public string? Label { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? JobUniqueCode { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedbyUsername { get; set; }
        public string? SubtenantName { get; set; }
        public Guid? TenantId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public class GetSubtenant
        {
            public Guid? Id { get; set; }
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
        }
    }
}
