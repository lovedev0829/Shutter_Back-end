using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class Pagination
    {
        public string SearchValue { get; set; } = "";
        public int PageNo { get; set; } = 1;
        public int RecordPerPage { get; set; } = 10;
    }
}
