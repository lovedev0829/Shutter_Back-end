using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class Pagination
    {
        [DefaultValue("")]
        public string? TenantId { get; set; }
        [DefaultValue("")]
        public string? SearchValue { get; set; }
        [DefaultValue(1)]
        public int PageNo { get; set; }
        [DefaultValue(10)]
        public int RecordPerPage { get; set; }
    }
}
