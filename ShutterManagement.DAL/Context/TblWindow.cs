using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblWindow
{
    public Guid WindowId { get; set; }

    public Guid? AddressId { get; set; }

    public string? Label { get; set; }

    public decimal? Width { get; set; }

    public decimal? Height { get; set; }

    public decimal? ArchHeight { get; set; }

    public string? Photo { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? TenantId { get; set; }

    public decimal? Quantity { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? JobId { get; set; }

    public bool? IsRated { get; set; }

    public virtual TblAddress? Address { get; set; }

    public virtual TblJob? Job { get; set; }

    public virtual ICollection<TblEstimateWindow> TblEstimateWindows { get; set; } = new List<TblEstimateWindow>();

    public virtual ICollection<TblWindowProduct> TblWindowProducts { get; set; } = new List<TblWindowProduct>();

    public virtual TblTenant? Tenant { get; set; }

    public virtual ICollection<WindowOption> WindowOptions { get; set; } = new List<WindowOption>();
}
