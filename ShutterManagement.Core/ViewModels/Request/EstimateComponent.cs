using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Core.ViewModels.Response;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class EstimateComponent
    {
        public Guid? EstimateId { get; set; }

        public List<WindowProdcutDetailsForEstimate> WindowProdcutDetailsForEstimate { get; set; }
    }
}
