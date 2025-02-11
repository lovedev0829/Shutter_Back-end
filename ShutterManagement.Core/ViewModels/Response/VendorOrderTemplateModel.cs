using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class VendorOrderTemplateModel
    {
            // Vendor Details
            public string? VendorId { get; set; }
            public string? VendorName { get; set; }
            public string? JobName { get; set; }
            public string? StreetAddress { get; set; }
            public string? VendorCity { get; set; }
            public string? VendorState { get; set; }
            public string? VendorZipCode { get; set; }
            public string? MobilePhone { get; set; }
            public string? EmailAddress { get; set; }

            // Order Details
            public string? OrderNo { get; set; }
            public string? OrderDate { get; set; }
            public string? ExpectedDeliveryDate { get; set; }
            public string? ActualDeliveryDate { get; set; }
            public string? VendorOrderStatus { get; set; }
            public decimal? TotalAmount { get; set; }
            public decimal BalanceDue { get; set; }
            public string? Notes { get; set; }

            // Company Details
            public string? TenantCompanyName { get; set; }
            public string? CompanyStreetAddress { get; set; }
            public string? CompanyCity { get; set; }
            public string? CompanyCountry { get; set; }
            public string? CompanyZipCode { get; set; }

            // Invoice Details
            public string? IssueDate { get; set; }
            public string? CreatedBy { get; set; }
            public string? Logo { get; set; }
            public string? Terms { get; set; }
            public string? VendorOrderItems { get; set; }

        // Order Items
        
        }

        public class VendorOrderItemModel
        {
            public string? MaterialName { get; set; }
            public int? Quantity { get; set; }
            public decimal? UnitPrice { get; set; }
            public decimal? TotalPrice { get; set; }
    }

    }

