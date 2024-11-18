using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblTenantBrandingInfo
{
    public Guid Id { get; set; }

    public string? PhoneNmber { get; set; }

    public string? Email { get; set; }

    public string? Logo { get; set; }

    public string? PrimaryColor { get; set; }

    public string? SecondaryColor { get; set; }

    public virtual ICollection<TblTenant> TblTenants { get; set; } = new List<TblTenant>();
}
