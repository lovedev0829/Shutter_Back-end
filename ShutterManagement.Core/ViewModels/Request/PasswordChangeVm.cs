using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class PasswordChangeVm
    {
        public string? PassWord { get; set; }
        public string? OldPassWord { get; set; }
        [Required]
        [Compare("PassWord", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassWord { get; set; }

    }
}
