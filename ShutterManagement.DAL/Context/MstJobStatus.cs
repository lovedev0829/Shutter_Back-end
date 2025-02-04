using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstJobStatus
{
    public Guid JobStatusId { get; set; }

    public string? JobStatusName { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsActive { get; set; }

    public string? Icon { get; set; }

    public virtual ICollection<TblJob> TblJobs { get; set; } = new List<TblJob>();
}
