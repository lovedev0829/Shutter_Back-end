using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class EstimateWindow
{
    public Guid EstimateWindowId { get; set; }

    public Guid? WindowId { get; set; }

    public Guid? AddressId { get; set; }

    public string? Label { get; set; }

    public decimal? Width { get; set; }

    public decimal? Height { get; set; }

    public decimal? ArchHeight { get; set; }

    public string? Photo { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? CreatedBy { get; set; }

    public virtual TblWindow? Window { get; set; }
}
