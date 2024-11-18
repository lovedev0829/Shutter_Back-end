using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblTaxExemption
{
    public Guid TaxExemptionId { get; set; }

    public Guid? ContactId { get; set; }

    public string? ExemptionCertificate { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public virtual TblContact? Contact { get; set; }
}
