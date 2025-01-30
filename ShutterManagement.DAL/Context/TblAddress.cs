using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblAddress
{
    public Guid AddressId { get; set; }

    public Guid? ContactId { get; set; }

    public string? Label { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public string? StreetAddress { get; set; }

    public string? Notes { get; set; }

    public string? AccessCode { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public virtual TblContact? Contact { get; set; }

    public virtual ICollection<TblEstimateWindow> TblEstimateWindows { get; set; } = new List<TblEstimateWindow>();

    public virtual ICollection<TblJob> TblJobs { get; set; } = new List<TblJob>();

    public virtual ICollection<TblMeasurement> TblMeasurements { get; set; } = new List<TblMeasurement>();

    public virtual ICollection<TblWindow> TblWindows { get; set; } = new List<TblWindow>();

    public virtual TblTenant? Tenant { get; set; }
}
