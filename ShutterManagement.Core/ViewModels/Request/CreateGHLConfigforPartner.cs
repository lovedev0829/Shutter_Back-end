using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateGHLConfigforPartner
    {
        public string? GHLUserId { get; set; }

        public bool? IsGHLConfigSameasTenant { get; set; }

        public string? PartnerId { get; set; }
    }

    public class GetGhlConfigforPartner
    {
        public string? GHLUserId { get; set; }
        public bool? IsGHLConfigSameasTenant { get; set; }
    }
}
