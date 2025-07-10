using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblRole
{
    public Guid Id { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public Guid? TenantId { get; set; }

    public string? RoleName { get; set; }

    public bool? IsShow { get; set; }

    public virtual ICollection<TblUserRole> TblUserRoles { get; set; } = new List<TblUserRole>();
}
