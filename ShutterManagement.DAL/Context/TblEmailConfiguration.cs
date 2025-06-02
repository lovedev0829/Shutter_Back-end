using System;
using System.Collections.Generic;

namespace ShutterManagement.DAL.Context;

public partial class TblEmailConfiguration
{
    public Guid Id { get; set; }

    public string? SmtpServer { get; set; }

    public int? Port { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool? UseSsl { get; set; }

    public string? ReplyTo { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? EmailType { get; set; }

    public Guid? ModifiedBy { get; set; }

    public Guid? TenantId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDelete { get; set; }

    public string? From { get; set; }

    public string? AccessToken { get; set; }
}
