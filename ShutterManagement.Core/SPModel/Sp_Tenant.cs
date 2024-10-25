using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel
{
     public class Sp_Tenant
    {
        public Guid TenantId { get; set; }
        public Guid SubTenantId { get; set; }
        public string TenantName { get; set; }
        public string  Brandinginfo { get; set; }
    }
}
