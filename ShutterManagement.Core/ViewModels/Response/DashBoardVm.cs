using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class DashBoardVm
    {
        public List<StatusCountVm>? StatusCounts { get; set; }
    }

    public class StatusCountVm
    {
        public string? Label { get; set; }
        public string? Count { get; set; }
        public string? Image { get; set; }
    }

}
