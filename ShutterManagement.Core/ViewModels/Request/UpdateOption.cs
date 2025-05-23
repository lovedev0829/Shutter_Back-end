using ShutterManagement.Core.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateOption
    {
        [DefaultValue("")]
        [Required]
        public Guid? OptionId { get; set; }
        [DefaultValue("")]
        [Required]
        public Guid? ProductId { get; set; }

        [DefaultValue("")]
        [Required]
        public Guid? ProductTypeId { get; set; }
        [DefaultValue("")]
        [Required]
        public string? OptionName { get; set; }
        public string? Unit { get; set; }
        public decimal? Markup { get; set; }
        public Guid? OptionExpression { get; set; }
        public OptionsType[]? Options { get; set; }

        public bool? IsTaxAvailable { get; set; }
        public Guid? TaxId { get; set; }


    }
   
}
