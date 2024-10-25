using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblAssembly
{
    public Guid AssemblyId { get; set; }

    public string? AssemblyName { get; set; }

    public string? ProductType { get; set; }

    public decimal? Width { get; set; }

    public decimal? Height { get; set; }

    public decimal? Length { get; set; }

    public decimal? Area { get; set; }

    public decimal? AdditionalWidth { get; set; }

    public decimal? ArchHeight { get; set; }

    public string? Color { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual ICollection<TblJobComponent> TblJobComponents { get; set; } = new List<TblJobComponent>();
}
