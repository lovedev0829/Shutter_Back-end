using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class LoginDetailVM
    {
        
        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? CompanyCode { get; set; }
    }
}
