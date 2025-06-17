using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblGhlCustomField
{
    public Guid Id { get; set; }

    public string? GhlFieldId { get; set; }

    public string? FieldName { get; set; }

    public string? Model { get; set; }

    public string? FieldKey { get; set; }

    public string? Placeholder { get; set; }

    public string? DataType { get; set; }

    public int? Position { get; set; }

    public string? DocumentType { get; set; }

    public string? ParentId { get; set; }

    public string? LocationId { get; set; }

    public DateTime? DateAdded { get; set; }

    public bool? IsStandard { get; set; }

    public Guid? TenantId { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }
}
