using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class GenerateInvoiceModel
    {
        public Guid? TemplateId { get; set; }
        public Guid? ContactId { get; set; }
        public Guid EstimationId { get; set; }
  
    }
}
