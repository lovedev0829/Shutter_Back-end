using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstJobType
{
    public Guid? Id { get; set; }

    public string? JobTypeName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }
}
