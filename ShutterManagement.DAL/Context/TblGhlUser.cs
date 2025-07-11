using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblGhlUser
{
    public Guid Id { get; set; }

    public string? GhlUserId { get; set; }

    public string? GhlUserName { get; set; }

    public string? GhlFirstName { get; set; }

    public string? GhlLastName { get; set; }

    public string? GhlEmail { get; set; }

    public string? GhlCalendarId { get; set; }

    public string? GhlCalendarName { get; set; }

    public DateTime? GhlLastSync { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? ModifiedBy { get; set; }
}
