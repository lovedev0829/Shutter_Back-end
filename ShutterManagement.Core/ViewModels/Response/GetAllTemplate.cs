using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllTemplate
    {
        public string? TemplateId { get; set; }
        public string? TemplateName { get; set; }
        public string? TemplateValue { get; set; }
        public bool? IsActive { get; set; }
    }
}
