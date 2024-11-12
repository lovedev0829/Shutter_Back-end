using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
     public class UpdateAddress : CreateAddress
     {
        public Guid AddressId { get; set; }


    }
}
