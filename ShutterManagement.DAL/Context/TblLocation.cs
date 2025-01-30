using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblLocation
{
    public Guid LocationId { get; set; }

    public string? LocationName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }
}
