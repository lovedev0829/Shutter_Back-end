using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UnitofMeasurementList
    {
        public Guid UnitofMeasurementId { get; set; }
        public string? UnitofMeasurementName { get; set; }
        public bool? IsActive { get; set; }
    }
}
