using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class DashBoardVm
    {
        public int? ContactsCount { get; set; }
        public decimal? TotalEstimateRevenue { get; set; }

        public List<StatusCountVm>? StatusCounts { get; set; }
    }

    public class StatusCountVm
    {
        public string? StatusName { get; set; }
        public int? TotalAwardedJobs { get; set; }
    }

}
