using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateSubtenantProductmarkup
    {
        public Guid? ProductId { get; set; }
        public decimal? Markup { get; set; }
    }
}
