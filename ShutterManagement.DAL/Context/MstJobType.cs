using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstJobType
{
    public Guid Id { get; set; }

    public string? JobTypeName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }
}
