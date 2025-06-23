using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblTemplate
{
    public Guid TemplateId { get; set; }

    public string? TemplateName { get; set; }

    public string? TemplateValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? TenantId { get; set; }

    public string? TemplateCategory { get; set; }

    public virtual TblTenant? Tenant { get; set; }
}
