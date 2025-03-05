using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class AdditionalProperty
    {
        public bool? IsSubtenant { get; set; } = false;
    }
}
