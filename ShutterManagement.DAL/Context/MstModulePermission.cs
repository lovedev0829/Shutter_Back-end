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

    public virtual MstModule? Module { get; set; }
}
