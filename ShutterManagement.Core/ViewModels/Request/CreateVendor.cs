using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateVendor
    {
        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? VendorName { get; set; }

        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? ContactInformation { get; set; }
        public bool? PreferredVendor { get; set; }
        public string? Notes { get; set; }
    }
}
