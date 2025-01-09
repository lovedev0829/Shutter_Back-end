using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblTenantBrandingInfo
{
    public Guid Id { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Logo { get; set; }

    public string? PrimaryColor { get; set; }

    public string? SecondaryColor { get; set; }

    public string? CompanyName { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public string? StreetAddress { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }

    public string? ZipCode { get; set; }

    public virtual ICollection<TblTenant> TblTenants { get; set; } = new List<TblTenant>();
}
