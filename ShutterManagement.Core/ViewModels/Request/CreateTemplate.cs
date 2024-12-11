using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateTemplate
    {
        public string? TemplateName { get; set; }
        public string? TemplateValue { get; set; }
    }
}
