using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
     public class UpdateMaterial : CreateMaterial
    {
        public string? MaterialId { get; set; } = string.Empty;
        public string? MaterialUniqueId { get; set; } = string.Empty;

      
    }
}
