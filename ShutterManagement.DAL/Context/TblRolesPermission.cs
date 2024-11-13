using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblRolesPermission
{
    public Guid? RoleId { get; set; }

    public Guid? ModulePermissionId { get; set; }

    public Guid Id { get; set; }

    public virtual MstModulePermission? ModulePermission { get; set; }

    public virtual TblRole? Role { get; set; }
}
