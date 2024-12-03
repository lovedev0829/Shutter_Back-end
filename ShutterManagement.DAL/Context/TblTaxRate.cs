using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblTaxRate
{
    public Guid TaxRateId { get; set; }

    public Guid? Region { get; set; }

    public decimal? Rate { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }
}
