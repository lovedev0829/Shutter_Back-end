using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class Tbttest
{
    public int? Id { get; set; }

    public string? Pwd { get; set; }

    public string? PasswordEncrypted { get; set; }
}
