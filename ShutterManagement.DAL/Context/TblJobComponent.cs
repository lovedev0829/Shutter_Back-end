using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblJobComponent
{
    public Guid JobComponentId { get; set; }

    public Guid? JobId { get; set; }

    public string? ComponentType { get; set; }

    public Guid? ComponentId { get; set; }

    public Guid? AssemblyId { get; set; }

    public string? Description { get; set; }

    public decimal? Quantity { get; set; }

    public DateTime? ProductionStartDate { get; set; }

    public DateTime? ProductionEndDate { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual TblProduct? Assembly { get; set; }

    public virtual TblComponent? Component { get; set; }

    public virtual TblJob? Job { get; set; }
}
