using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblContactBillingAddress
{
    public Guid BillingAddressId { get; set; }

    public Guid? ContactId { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    public bool IsDelete { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? StreetAddress { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public virtual TblContact? Contact { get; set; }
}
