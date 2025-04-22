using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblGhlconfiguration
{
    public Guid Id { get; set; }

    public string? PipelineId { get; set; }

    public Guid? TenantId { get; set; }

    public string? LocationId { get; set; }

    public DateTime? LastSync { get; set; }

    public string? GeneratedToken { get; set; }

    public string? RefreshToken { get; set; }

    public string? AccessToken { get; set; }

    public DateTime? RefreshExpirationtime { get; set; }

    public DateTime? AccessExpirationtime { get; set; }

    public string? CalendarName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? ModifiedBy { get; set; }
}
