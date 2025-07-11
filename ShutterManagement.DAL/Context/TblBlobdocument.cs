using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblBlobdocument
{
    public Guid DocumentId { get; set; }

    public string? Title { get; set; }

    public Guid? JobId { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public string? BlobUrl { get; set; }
}
