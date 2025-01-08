using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class InvoiceDataModel
    {
        public string? CustomerName { get; set; }
        public string? PreparedBy { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerCity { get; set; }
        public string? CustomerState { get; set; }
        public string? CustomerZipCode { get; set; }
        public string? HomePhone { get; set; }
        public string? CompanyStreetAddress { get; set; }
        public string? MobilePhone { get; set; }
        public string? TenantCompanyName { get; set; }
        public string? Logo { get; set; }
        public string? EstimationDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? EstimationSubtotal { get; set; }
        public string? EstimationGrandTotal { get; set; }
        public string? EstimationId { get; set; }
        public string? EmailAddress { get; set; }
        public string? ProductListHtml { get; set; }
    }
}
