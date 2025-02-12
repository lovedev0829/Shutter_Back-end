using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class WelcomeEmailModel
    {
        public string? CompanyName { get; set; }
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Logo { get; set; }
        public string? loginUrl { get; set; }
        public string? color { get; set; }
    }

}
