using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblEmailType
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? IsOauth { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }
}
