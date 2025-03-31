using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class ImportMaterial
    {
        [DefaultValue("")]
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+={}\[\]:;""'<>,.?/\|`~ \s-]*$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? MaterialName { get; set; }

        [DefaultValue("")]
        [Required]
        //[OnError(AppConstant.NumericError)]
        public decimal? UnitCost { get; set; }

        [DefaultValue("")]
        [Required]
        public string? UnitOfMeasurement { get; set; }

        public decimal? NonRatedUnitCost { get; set; }

        [DefaultValue("")]
        [Required]
        public decimal? StockQuantity { get; set; }

        [DefaultValue("")]
        [Required]
        public decimal? ReorderLevel { get; set; }
        [Required]
        public string? ProductType { get; set; }
        [DefaultValue("")]
        [Required]

        public string? VendorCode { get; set; }

        public string SKU { get; set; }

        public bool IsCutSheet { get; set; } = false;

        [MaxLength(1000)]
        public string? Notes { get; set; }

       
    }
}
