using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblSubtenantProductMarkup
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public decimal? Markup { get; set; }

    public bool? IsActive { get; set; }

    public bool? Isdelete { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public Guid? ParentTenantId { get; set; }

    public Guid? ProductId { get; set; }
}
