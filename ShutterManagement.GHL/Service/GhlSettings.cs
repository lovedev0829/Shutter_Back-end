using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.GHL.Service
{
    public class GhlSettings
    {
        public string ApiKey { get; set; }
        public string PipelineId { get; set; }
        public string StageId { get; set; }
        public string BaseAddress { get; set; }
    }

}
