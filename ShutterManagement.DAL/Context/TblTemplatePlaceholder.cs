using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblTemplatePlaceholder
{
    public Guid Id { get; set; }

    public Guid? TemplateId { get; set; }

    public Guid? PlaceholderId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? TenantId { get; set; }
}
