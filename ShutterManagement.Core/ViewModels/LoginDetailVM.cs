using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class LoginDetailVM
    {
        [DefaultValue("")]
        [Required]
        public string? Username { get; set; }

        [DefaultValue("")]
        [Required]
        public string? Password { get; set; }

        [DefaultValue("")]
        [Required]
        public string? CompanyCode { get; set; }
    }
}
