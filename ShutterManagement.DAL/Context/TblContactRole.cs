using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblContactRole
{
    public Guid ContactRoleId { get; set; }

    public Guid? ContactId { get; set; }

    public string? RoleType { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual TblContact? Contact { get; set; }
}
