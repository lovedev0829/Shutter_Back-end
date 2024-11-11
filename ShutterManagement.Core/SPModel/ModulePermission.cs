using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel
{
    public class ModulePermissions
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string ModulePermission { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }

        public Module Module { get; set; }
    }
}
