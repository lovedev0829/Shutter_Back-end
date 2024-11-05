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

    public virtual ICollection<TblUserRole> TblUserRoles { get; set; } = new List<TblUserRole>();

    public virtual TblTenant? Tenant { get; set; }
}
