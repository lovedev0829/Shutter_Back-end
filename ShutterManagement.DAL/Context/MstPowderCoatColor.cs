using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstPowderCoatColor
{
    public Guid Id { get; set; }

    public string ColorName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool IsDelete { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public decimal? Cost { get; set; }

    public virtual ICollection<TblJob> TblJobs { get; set; } = new List<TblJob>();
}
