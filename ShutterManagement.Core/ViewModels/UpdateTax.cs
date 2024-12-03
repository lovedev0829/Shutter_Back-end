using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class UpdateTax : CreateTax
    {
        public Guid? TaxRateId { get; set; }
    }
}
