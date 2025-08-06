using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblEstimateApproved
{
    public Guid Id { get; set; }

    public Guid? EstimationId { get; set; }

    public DateTime? CurrentDatetime { get; set; }

    public string? Ipaddress { get; set; }

    public string? Status { get; set; }

    public Guid? UpdatedBy { get; set; }
}
