using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class EstimateAmountVm
    {
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal DueBalance { get; set; }
        public decimal HalfAmount { get; set; }
        public decimal ExtraBalance { get; set; }

        public List<InvoicePayment_VM> Invoices { get; set; }

        

    }


}
