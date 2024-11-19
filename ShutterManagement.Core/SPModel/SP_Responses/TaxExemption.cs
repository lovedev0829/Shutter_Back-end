using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel.SP_Responses
{
    public class TaxExemption
    {
        [DefaultValue("")]
        public string ExemptionCertificate { get; set; } = string.Empty;

        [DefaultValue("")]
        public DateTime EffectiveDate { get; set; }

        [DefaultValue("")]
        public DateTime ExpiryDate { get; set; }
    }
}
