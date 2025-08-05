using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblNotificationTemplate
{
    public Guid Id { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public string? Type { get; set; }
}
