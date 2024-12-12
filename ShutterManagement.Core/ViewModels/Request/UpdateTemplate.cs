using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateTemplate : CreateTemplate
    {
        public string? TemplateId { get; set; }
    }
}
