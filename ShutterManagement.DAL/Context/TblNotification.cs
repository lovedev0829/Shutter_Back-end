using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblNotification
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public DateTime? Createdate { get; set; }

    public bool? IsReaded { get; set; }

    public string? Type { get; set; }

    public Guid? TypeId { get; set; }
}
