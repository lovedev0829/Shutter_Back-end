using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class GenerateInvoiceModel
    {
        public Guid? TemplateId { get; set; } = Guid.Parse("DFBD5BD7-D9F9-4E41-8B48-375B3BA79610");
        [Required]
        public Guid? ContactId { get; set; }
        [Required]
        public Guid EstimationId { get; set; }
  
    }
}
