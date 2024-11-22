using ShutterManagement.Core.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class CreateProductComponentVm
    {
        public Guid ProductId { get; set; }
        public List<ProductComponentVm> ProductComponents { get; set; }
    }
}
