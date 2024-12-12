using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateOption
    {
        [DefaultValue("")]
        public Guid? ProductTypeId { get; set; }
        [DefaultValue("")]
        public string? OptionName { get; set; }
        public Options[]? Options { get; set; }
    }

    public class Options
    {
        [DefaultValue("")]
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+={}\[\]:;""'<>,.?/\|`~ \s-]*$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? OptionType { get; set; }

        //[DefaultValue("")]
        //[Required]
        public decimal? AdditionalCost { get; set; }

        public bool? IsDefault { get; set; }
    }
}
