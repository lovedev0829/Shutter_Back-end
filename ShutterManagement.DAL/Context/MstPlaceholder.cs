using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstPlaceholder
{
    public Guid? Id { get; set; }

    public string? PlaceholderValue { get; set; }

    public string? Dbvalue { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }
}
