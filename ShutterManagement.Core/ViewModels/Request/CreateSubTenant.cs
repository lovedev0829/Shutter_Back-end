using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateSubTenant : Tenant
    {
        public string? ParentTenantId { get; set; }
    }
}
