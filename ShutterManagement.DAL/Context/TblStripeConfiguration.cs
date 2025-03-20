using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblStripeConfiguration
{
    public Guid Id { get; set; }

    public string? SecretKey { get; set; }

    public string? PublishableKey { get; set; }

    public string? CancelUrl { get; set; }

    public string? SuccessUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ModifiedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public string? WebhookSecret { get; set; }
}
