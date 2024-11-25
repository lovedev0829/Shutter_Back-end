using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class MstUnitofMeasurement
{
    public Guid UnitofMeasurementId { get; set; }

    public string? UnitofMeasurementName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }
}
