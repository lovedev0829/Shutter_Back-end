using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class ForgetPassword
    {
        [DefaultValue("")]
        [Required]
        public string? TenantId { get; set; }

        [DefaultValue("")]
        [Required]
        public string? Email { get; set; }
    }
}
