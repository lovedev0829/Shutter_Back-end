using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblContactBillingAddress
{
    public Guid BillingAddressId { get; set; }

    public Guid? ContactId { get; set; }

    public string? Label { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string? Country { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    public bool IsDelete { get; set; }

    public virtual TblContact? Contact { get; set; }
}
