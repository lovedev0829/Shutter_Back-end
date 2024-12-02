using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateUser : UserDetails
    {
        public string? UserId { get; set; }
        [DefaultValue("")]
        [MinLength(8), MaxLength(12)]
        public string? Password { get; set; }
    }
}
