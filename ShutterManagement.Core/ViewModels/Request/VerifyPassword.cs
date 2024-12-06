using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class VerifyPassword
    {
        public string? Token { get; set; }

        [DefaultValue("")]
        [Required]
        [MinLength(8), MaxLength(12)]
        public string? Password { get; set; }
    }
}
