using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblWindowProduct
{
    public Guid WindowProductId { get; set; }

    public Guid? WindowId { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? ProductTypeId { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public decimal? BuildWidth { get; set; }

    public decimal? BuildHeight { get; set; }

    public virtual TblProduct? Product { get; set; }

    public virtual MstProductType? ProductType { get; set; }

    public virtual ICollection<TblEstimateWindowProductOption> TblEstimateWindowProductOptions { get; set; } = new List<TblEstimateWindowProductOption>();

    public virtual ICollection<TblEstimateWindowProduct> TblEstimateWindowProducts { get; set; } = new List<TblEstimateWindowProduct>();

    public virtual ICollection<TblOrderWindowProductOption> TblOrderWindowProductOptions { get; set; } = new List<TblOrderWindowProductOption>();

    public virtual TblTenant? Tenant { get; set; }

    public virtual TblWindow? Window { get; set; }

    public virtual ICollection<WindowOption> WindowOptions { get; set; } = new List<WindowOption>();
}
