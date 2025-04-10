using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblJobStageStatusAutomAtionHistory
{
    public Guid HistoryId { get; set; }

    public string? JobJson { get; set; }

    public Guid? ChangedBy { get; set; }

    public DateTime? ChangedAt { get; set; }

    public Guid? TenantId { get; set; }
}
