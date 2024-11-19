using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateOption : CreateOption
    {
        [DefaultValue("")]
        public Guid? OptionId { get; set; } 
    }
}
