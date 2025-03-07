using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblGhlStage
{
    public string? GhlStageId { get; set; }

    public string? GhlStageName { get; set; }

    public string? GhlPipelineId { get; set; }

    public DateTime? GhlLastSync { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? GhlPipelineName { get; set; }
}
