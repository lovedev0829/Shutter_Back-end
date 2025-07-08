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
        [Required]
        public Guid? ProductId { get; set; }
        [DefaultValue("")]
        [Required]
        public Guid? ProductTypeId { get; set; }
        [DefaultValue("")]
        [Required]
        public string? OptionName { get; set; }
        public decimal? Markup { get; set; }
        public Guid? OptionExpression { get; set; }

        [DefaultValue("")]
        [Required]
        public string? Unit { get; set; }
        public Options[]? Options { get; set; }

        public bool? IsTaxAvailable { get; set; }
        public Guid? TaxId { get; set; }
        public bool? BuildRequirement { get; set; }
        public bool? UseInCutSheet { get; set; }
    }

    public class Options
    {
        [DefaultValue("")]
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+={}\[\]:;""'<>,.?/\|`~ \s-]*$", ErrorMessage = AppConstant.AlphabeticalError)]
        public string? OptionType { get; set; }

        [DefaultValue(null)]
        public decimal? AdditionalCost { get; set; }

        public bool? IsDefault { get; set; }
        public int? SortOrder { get; set; }
        public bool? RequirePriceAtEstimatePreview { get; set; }
    }
}
