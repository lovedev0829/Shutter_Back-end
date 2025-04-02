using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllStateVm
    {

        public Guid? StateId { get; set; }
        public string? StateName { get; set; }

        public bool? IsActive { get; set; }


    }
}
