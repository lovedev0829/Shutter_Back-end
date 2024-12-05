using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstContactType
{
    public Guid MstContactId { get; set; }

    public string? MstContactName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }
}
