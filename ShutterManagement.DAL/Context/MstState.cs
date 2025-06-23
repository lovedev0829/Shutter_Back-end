using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstState
{
    public Guid Id { get; set; }

    public string? StateName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public string? StateCode { get; set; }

    public virtual ICollection<TblAddress> TblAddresses { get; set; } = new List<TblAddress>();

    public virtual ICollection<TblVendor> TblVendors { get; set; } = new List<TblVendor>();
}
