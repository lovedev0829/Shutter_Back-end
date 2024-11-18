using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstProductType
{
    public Guid ProductId { get; set; }

    public string? ProductName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual ICollection<TblOption> TblOptions { get; set; } = new List<TblOption>();
}
