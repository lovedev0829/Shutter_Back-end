using ShutterManagement.Core.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateOption
    {
        [DefaultValue("")]
        public Guid? OptionId { get; set; }
        [DefaultValue("")]
        public Guid? ProductTypeId { get; set; }
        [DefaultValue("")]
        public string? OptionName { get; set; }
        public OptionsType[]? Options { get; set; }

    }
   
}
