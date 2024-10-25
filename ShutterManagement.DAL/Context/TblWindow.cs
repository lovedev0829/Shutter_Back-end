using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblWindow
{
    public Guid WindowId { get; set; }

    public Guid? AddressId { get; set; }

    public string? Label { get; set; }

    public string? Location { get; set; }

    public decimal? Width { get; set; }

    public decimal? Height { get; set; }

    public decimal? ArchHeight { get; set; }

    public string? Photo { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual TblAddress? Address { get; set; }
}
