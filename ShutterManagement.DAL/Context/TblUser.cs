using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblUser
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Pwd { get; set; } = null!;
}
