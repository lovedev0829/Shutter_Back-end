using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllRegion
    {
        public Guid? RegionId { get; set; }
        public string? RegionName { get; set; }

        public bool? IsActive { get; set; }

    }
}
