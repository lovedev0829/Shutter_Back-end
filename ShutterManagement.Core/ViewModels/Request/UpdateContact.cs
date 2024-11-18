using ShutterManagement.Core.SPModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateContact : Sp_Contact
    {
        public Guid ContactId { get; set; } = Guid.Empty;

    }
}
