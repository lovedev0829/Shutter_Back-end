using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstModulePermission
{
    public Guid Id { get; set; }

    public Guid? ModuleId { get; set; }

    public string? ModulePermission { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public string? PermissionDisplayName { get; set; }

    public string? Icon { get; set; }

    public string? Url { get; set; }

    public int? SortOrder { get; set; }

    public virtual MstModule? Module { get; set; }

    public virtual ICollection<TblRolesPermission> TblRolesPermissions { get; set; } = new List<TblRolesPermission>();
}
