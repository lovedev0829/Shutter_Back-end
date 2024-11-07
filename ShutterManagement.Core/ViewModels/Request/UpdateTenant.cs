using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateTenant : CreateTenantRequest
    {
        public string? TenantId { get; set; }

        public string? ParentTenantId { get; set; }

    }
}
