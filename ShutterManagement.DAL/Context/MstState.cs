using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstState
{
    public Guid Id { get; set; }

    public string? StateName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }
}
