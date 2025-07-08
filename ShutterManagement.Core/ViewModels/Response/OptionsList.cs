using ShutterManagement.Core.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class OptionsList
    {
        public Guid? OptionId { get; set; }
        public string? OptionName { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ProductTypeId { get; set; }
        public string? ProductName { get; set; }
        public decimal? Markup { get; set; }
        public string? OptionExpression { get; set; }
        public string? ProductTypeName { get; set; }
        public bool? IsActive { get; set; }
        public string? Unit { get; set; }
        public List<OptionsType>? Options { get; set; }

        public bool? IsTaxAvailable { get; set; } = true;
        public Guid? TaxId { get; set; }

        public string? TaxName { get; set; }
        public bool? BuildRequirement { get; set; }
        public bool? UseInCutSheet { get; set; }
    }
    public class OptionsType
    {
        public Guid? OptionTypeId { get; set; }
        public string? OptionType { get; set; }

        [DefaultValue("")]
        public decimal? AdditionalCost { get; set; }
        public bool? IsDefault { get; set; }
        public int? SortOrder { get; set; }
        public bool? RequirePriceAtEstimatePreview { get; set; }

    }
}
