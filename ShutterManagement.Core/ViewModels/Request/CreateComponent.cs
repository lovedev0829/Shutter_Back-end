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
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+={}\[\]:;""'<>,.?/\|`~ \s-]*$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? ComponentName { get; set; }

        [DefaultValue("")]
        [Required]
        //[RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? ProductType { get; set; }

        [DefaultValue("")]
        [Required]
        public decimal? MarkupPercentage { get; set; }

        //[Required]
        //public Guid? ExpressionId { get; set; }

        [DefaultValue("")]
       // [Required]
        [MaxLength(500)]
        public string? PricingExpression { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public List<ComponentMaterialModel>? Materials { get; set; }

    }

    public class ComponentMaterialModel
    {
        [DefaultValue("")]
        public Guid MaterialId { get; set; }

        public decimal? Qty { get; set; }
        public decimal? WasteFactor { get; set; }
    }
}
