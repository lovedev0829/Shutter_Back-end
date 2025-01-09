using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class SettingUpdate
    {

        public string? Logo { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? CompanyName { get; set; }

    }
}
