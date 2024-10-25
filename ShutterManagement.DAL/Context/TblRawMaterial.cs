using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblRawMaterial
{
    public Guid RawMaterialId { get; set; }

    public string? MaterialName { get; set; }

    public decimal? UnitCost { get; set; }

    public string? UnitOfMeasurement { get; set; }

    public int? StockQuantity { get; set; }

    public int? ReorderLevel { get; set; }

    public Guid? VendorId { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual ICollection<TblComponentMaterial> TblComponentMaterials { get; set; } = new List<TblComponentMaterial>();

    public virtual TblContact? Vendor { get; set; }
}
