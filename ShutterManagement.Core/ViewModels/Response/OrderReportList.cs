using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class OrderReportList
    {
        public Guid? EstimateId { get; set; }
        public string? OrderNo { get; set; }
        public string? ContactName { get; set; }
        public DateTime? Orderdate { get; set; }
        public DateTime? Confirmdate { get; set; }
        public decimal? Totalamnt { get; set; }
    }
}
