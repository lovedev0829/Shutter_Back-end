using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateMaterial
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

        [DefaultValue("")]
        [Required]
        public decimal? StockQuantity { get; set; }

        [DefaultValue("")]
        //  [Required]
        public decimal? ReorderLevel { get; set; } = 0m;
        [Required]
        [DefaultValue("")]
        public string? ProductType { get; set; }
        [DefaultValue("")]
        [Required]
      

        public Guid? VendorId { get; set; }

        [DefaultValue("")]
        [RegularExpression(@"^[A-Za-z0-9_-]+", ErrorMessage = AppConstant.SKUError)]
        [Required]
        public string SKU { get; set; }

        public bool IsCutSheet { get; set; } = false;

        [DefaultValue("")]
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
