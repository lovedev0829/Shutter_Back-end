using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblAppointmentSchedule
{
    public Guid Id { get; set; }

    public Guid? JobId { get; set; }

    public DateTime? ScheduleDateTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? GhlUserId { get; set; }

    public DateTime? LastSync { get; set; }

    public string? GhlEventId { get; set; }

    public Guid? UserId { get; set; }

    public virtual TblJob? Job { get; set; }

    public virtual TblUser? User { get; set; }
}
