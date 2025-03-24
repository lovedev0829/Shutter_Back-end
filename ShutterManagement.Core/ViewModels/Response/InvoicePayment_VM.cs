using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class InvoicePayment_VM :InvoiceVM
    {
        public List<PaymentsVM> Payments { get; set; }
    }
}
