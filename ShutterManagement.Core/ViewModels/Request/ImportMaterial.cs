using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class ImportMaterial
    {
        [DefaultValue("")]
        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9!@#$%^&*()_+={}\[\]:;""'<>,.?/\|`~ \s-]*$", ErrorMessage = AppConstant.AlphabaticalError)]
        [Name("MaterialName*")]
        public string? MaterialName { get; set; }

        [Name("MaterialUniqueId")]
        public string? MaterialUniqueId { get; set; }

        [DefaultValue("")]
        [Required]
        //[OnError(AppConstant.NumericError)]
        [Name("UnitCost*")]
        public decimal? UnitCost { get; set; }

        [DefaultValue("")]
        [Required]
        [Name("UnitOfMeasurement*")]
        public string? UnitOfMeasurement { get; set; }

        [Name("NonRatedUnitCost*")]
        public decimal? NonRatedUnitCost { get; set; }

        public decimal? StockQuantity { get; set; }

        public decimal? Lenght { get; set; }
        public decimal? Width { get; set; }

        [DefaultValue("")]
        [Required]
        public decimal? ReorderLevel { get; set; }
        [Required]
        [Name("ProductType*")]
        public string? ProductType { get; set; }
        [DefaultValue("")]
        [Required]
        [Name("Vendor*")]
        public string? Vendor { get; set; }

        public string SKU { get; set; }

        [Name("IsCutSheet*")]
        public bool IsCutSheet { get; set; } = false;

        [MaxLength(1000)]
        public string? Notes { get; set; }

       
    }
}
