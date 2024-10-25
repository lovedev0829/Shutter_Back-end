using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblMeasurement
{
    public Guid MeasurementId { get; set; }

    public Guid? AddressId { get; set; }

    public string? ProductType { get; set; }

    public string? Measurements { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public virtual TblAddress? Address { get; set; }
}
