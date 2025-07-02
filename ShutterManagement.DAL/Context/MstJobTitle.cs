using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstJobTitle
{
    public Guid Id { get; set; }

    public string? JobTitleName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public virtual ICollection<TblJobSchedule> TblJobSchedules { get; set; } = new List<TblJobSchedule>();

    public virtual ICollection<TblUser> TblUsers { get; set; } = new List<TblUser>();
}
