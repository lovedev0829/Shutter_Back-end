using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class LoginVm
    {
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid Username.")]
        [EmailAddress(ErrorMessage = "Invalid Username")]
        public string Username { get; set; }

        public string Password { get; set; }

        public string Code { get; set; }
    }
}
