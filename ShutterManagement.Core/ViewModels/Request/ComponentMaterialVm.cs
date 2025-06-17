using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateComponentMaterialVm
    {
        [DefaultValue("")]
        public Guid ComponentId { get; set; }

       // public Guid MaterialPricingExpression { get; set; }

        public List<ComponentMaterialModel>? Materials { get; set; }
    }



}
