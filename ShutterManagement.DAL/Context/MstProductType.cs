using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstProductType
{
    public Guid ProductId { get; set; }

    public string? ProductName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TblEstimateWindowProductComponentMaterial> TblEstimateWindowProductComponentMaterials { get; set; } = new List<TblEstimateWindowProductComponentMaterial>();

    public virtual ICollection<TblOption> TblOptions { get; set; } = new List<TblOption>();

    public virtual ICollection<TblRawMaterial> TblRawMaterials { get; set; } = new List<TblRawMaterial>();

    public virtual ICollection<TblWindowProduct> TblWindowProducts { get; set; } = new List<TblWindowProduct>();
}
