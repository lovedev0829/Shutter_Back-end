using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateComponentMaterialVm
    {
        public Guid ComponentId { get; set; }

        public List<ComponentMaterialModel> Materials { get; set; }
    }



}
