using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateComponent
    {
        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? ComponentName { get; set; }

        [DefaultValue("")]
        [Required]
        //[RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? ProductType { get; set; }

        [DefaultValue("")]
        [Required]
        public decimal? MarkupPercentage { get; set; }

        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        public string? PricingExpression { get; set; }

        public string? Notes { get; set; }

    }
}
