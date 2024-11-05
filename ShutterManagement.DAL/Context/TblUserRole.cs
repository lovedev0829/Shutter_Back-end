using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblUserRole
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public Guid? RoleId { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual TblRole? Role { get; set; }

    public virtual TblUser? User { get; set; }
}
