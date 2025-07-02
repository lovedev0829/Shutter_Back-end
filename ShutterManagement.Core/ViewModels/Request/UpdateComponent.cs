using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateComponent 
    {
        [DefaultValue("")]
        public string ComponentId { get; set; } = string.Empty;

        [DefaultValue("")]
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+={}\[\]:;""'<>,.?/\|`~ \s-]*$", ErrorMessage = AppConstant.AlphabeticalError)]
        public string? ComponentName { get; set; }

        [DefaultValue("")]
        [Required]
        //[RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabeticalError)]
        public string? ProductType { get; set; }

        [DefaultValue("")]
        [Required]
        public decimal? MarkupPercentage { get; set; }

        //[Required]
        //public Guid? ExpressionId { get; set; }

        //[DefaultValue("")]
        //[Required]
        //[MaxLength(500)]
        //public string? PricingExpression { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
