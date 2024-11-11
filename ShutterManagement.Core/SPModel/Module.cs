using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel
{
    public class Module
    {
        public int Id { get; set; }
        public string ModuleName { get; set; }
        public string ModuleDisplayName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }

        public ICollection<ModulePermissions> ModulePermissions { get; set; }
    }
}
