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
        public string? OptionType { get; set; }


        public Options[]? Options { get; set; }
    }

    public class Options
    {
        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? OptionName { get; set; }

        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        public decimal? AdditionalCost { get; set; }

        public bool? IsDefault { get; set; }
    }
}
