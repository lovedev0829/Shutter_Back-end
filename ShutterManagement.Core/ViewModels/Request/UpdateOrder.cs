using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateOrder : CreateOrder
    {
        [DefaultValue("")]
        [Required]
        public Guid OrderId { get; set; }
    }
}
