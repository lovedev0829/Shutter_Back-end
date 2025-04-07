using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblGhlStagesmapping
{
    public Guid Id { get; set; }

    public Guid? StatusId { get; set; }

    public Guid? StageId { get; set; }

    public Guid? TenantId { get; set; }
}
