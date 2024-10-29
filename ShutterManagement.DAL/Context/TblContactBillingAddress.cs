using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblContactBillingAddress
{
    public string? FullName { get; set; }

    public string? EmailAddress { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? ContactId { get; set; }

    public virtual TblContact? Contact { get; set; }
}
