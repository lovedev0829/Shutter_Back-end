using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblGhlconfiguration
{
    public Guid Id { get; set; }

    public string? Apikey { get; set; }

    public string? PipelineId { get; set; }

    public string? StageId { get; set; }

    public string? BaseAddress { get; set; }

    public string? TenantId { get; set; }
}
