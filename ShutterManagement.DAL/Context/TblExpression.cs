using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblExpression
{
    public Guid Id { get; set; }

    public string? ExpressionName { get; set; }

    public string? ExpressionFormulae { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? ProductType { get; set; }

    public string? FormulaeArray { get; set; }

    public string? DisplayArray { get; set; }


    public bool? IsOptionExpression { get; set; }

    public virtual MstProductType? ProductTypeNavigation { get; set; }
}
