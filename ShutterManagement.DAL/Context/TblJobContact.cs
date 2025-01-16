using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblJobContact
{
    public Guid JobContactId { get; set; }

    public Guid? ContactTypeId { get; set; }

    public string? ContactName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? JobId { get; set; }

    public Guid? ContactId { get; set; }

    public virtual TblContact? Contact { get; set; }

    public virtual MstContactType? ContactType { get; set; }

    public virtual TblJob? Job { get; set; }
}
