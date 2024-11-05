using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblRole
{
    public Guid Id { get; set; }

    public Guid? ModuleId { get; set; }

    public Guid? ModulePermissionId { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public Guid? TenantId { get; set; }

    public virtual MstModule? Module { get; set; }

    public virtual MstModulePermission? ModulePermission { get; set; }

    public virtual ICollection<TblUserRole> TblUserRoles { get; set; } = new List<TblUserRole>();

    public virtual TblTenant? Tenant { get; set; }
}
