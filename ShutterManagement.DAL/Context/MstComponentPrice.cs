using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstComponentPrice
{
    public Guid Id { get; set; }

    public string? ProductType { get; set; }

    public string? ComponentName { get; set; }

    public decimal? PriceRated { get; set; }

    public decimal? PriceNonRated { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }
}
