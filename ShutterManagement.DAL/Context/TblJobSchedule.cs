using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblJobSchedule
{
    public Guid JobScheduleId { get; set; }

    public Guid? JobTitleId { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public TimeOnly? DayStartTime { get; set; }

    public TimeOnly? DayEndDateTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? JobId { get; set; }

    public Guid? EstimateId { get; set; }

    public string? GhlEventId { get; set; }

    public DateTime? LasSync { get; set; }

    public virtual TblEstimate? Estimate { get; set; }

    public virtual TblJob? Job { get; set; }

    public virtual MstJobTitle? JobTitle { get; set; }

    public virtual TblUser? User { get; set; }
}
