using ShutterManagement.Core.SPModel.SP_Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class UserLoginDetails : LoginDetails
    {
        public string? Token { get; set; }
        public LoginRoles[]? Roles { get; set; }
        public string[]? Menu { get; set; }
    }
}
