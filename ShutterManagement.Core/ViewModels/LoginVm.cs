using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class LoginVm
    {
        public Guid Id { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }

        public string Code { get; set; }
    }
}
