using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstRegion
{
    public Guid? Id { get; set; }

    public string? RegionName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }
}
