using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstModule
{
    public Guid Id { get; set; }

    public string? ModuleName { get; set; }

    public string? ModuleDisplayName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual ICollection<MstModulePermission> MstModulePermissions { get; set; } = new List<MstModulePermission>();
}
