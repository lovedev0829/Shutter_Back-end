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
        public string? TenantId { get; set; } = "";
        [DefaultValue("")]
        public string? SearchValue { get; set; } = "";
        [DefaultValue(1)]
        public int PageNo { get; set; } = 1;
        [DefaultValue(10)]
        public int RecordPerPage { get; set; } = 10;

        [DefaultValue(null)]
        public DateTime? StartDate { get; set; } = null;

        [DefaultValue(null)]
        public DateTime? ExpiryDate { get; set; } = null;
        [DefaultValue(2)]
        public int Status { get; set; } = 2;
        [DefaultValue("")]
        public string? Vendor { get; set; } = "";
        [DefaultValue("")]
        public string? CustomerId { get; set; } = "";
        [DefaultValue("")]
        public string? ProductType { get; set; } = "";
        [DefaultValue("")]
        public string? JobStatus { get; set; } = "";

        [DefaultValue("")]
        public string? Address { get; set; } = "";

        [DefaultValue("")]
        public string? Region { get; set; } = "";
    }
}
