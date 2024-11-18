using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblJobStatus
{
    public string? StatusName { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public Guid StatusId { get; set; }

    public virtual ICollection<TblJob> TblJobs { get; set; } = new List<TblJob>();
}
