using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblUser
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Pwd { get; set; } = null!;

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? TenantId { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Token { get; set; }

    public DateTime? ExpireAt { get; set; }

    public Guid? JobTitleId { get; set; }

    public string? ForgetPassword { get; set; }

    public string? Suffix { get; set; }

    public bool? IsTenantAdmin { get; set; }

    public string? GhlUserId { get; set; }

    public virtual MstJobTitle? JobTitle { get; set; }

    public virtual ICollection<TblAppointmentSchedule> TblAppointmentSchedules { get; set; } = new List<TblAppointmentSchedule>();

    public virtual ICollection<TblJobSchedule> TblJobSchedules { get; set; } = new List<TblJobSchedule>();

    public virtual ICollection<TblUserRole> TblUserRoles { get; set; } = new List<TblUserRole>();

    public virtual TblTenant? Tenant { get; set; }
}
