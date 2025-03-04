using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblJob
{
    public Guid JobId { get; set; }

    public Guid? ContactId { get; set; }

    public Guid? AddressId { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? AssignedPersonnel { get; set; }

    public string? AttachmentAny { get; set; }

    public Guid? JobStatus { get; set; }

    public Guid? JobType { get; set; }

    public string? JobName { get; set; }

    public string? JobUniqueId { get; set; }

    public Guid? GhlStageId { get; set; }

    public string? GhlStageName { get; set; }

    public virtual TblAddress? Address { get; set; }

    public virtual TblContact? Contact { get; set; }

    public virtual MstJobStatus? JobStatusNavigation { get; set; }

    public virtual ICollection<TblEstimate> TblEstimates { get; set; } = new List<TblEstimate>();

    public virtual ICollection<TblInvoice> TblInvoices { get; set; } = new List<TblInvoice>();

    public virtual ICollection<TblJobComponent> TblJobComponents { get; set; } = new List<TblJobComponent>();

    public virtual ICollection<TblJobContact> TblJobContacts { get; set; } = new List<TblJobContact>();

    public virtual ICollection<TblJobSchedule> TblJobSchedules { get; set; } = new List<TblJobSchedule>();

    public virtual ICollection<TblVendorOrder> TblVendorOrders { get; set; } = new List<TblVendorOrder>();

    public virtual ICollection<TblWindow> TblWindows { get; set; } = new List<TblWindow>();

    public virtual TblTenant? Tenant { get; set; }
}
