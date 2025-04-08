using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class StageMapping
    {
        public Guid? StatusId { get; set; }
        public Guid? StageId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class StageMappingRequest
    {
        public List<StageMapping> Mappings { get; set; }
    }

}
