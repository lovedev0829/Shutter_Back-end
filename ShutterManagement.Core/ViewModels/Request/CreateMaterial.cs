using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public decimal? UnitCost { get; set; }

        [DefaultValue("")]
        [Required]
        public string? UnitOfMeasurement { get; set; }

        [DefaultValue("")]
        [Required]
        public int? StockQuantity { get; set; }

        [DefaultValue("")]
        [Required]
        public int? ReorderLevel { get; set; }

        [DefaultValue("")]
        [Required]
      
        public Guid? VendorId { get; set; }

        public string SKU { get; set; }

        public bool IsCutSheet { get; set; } = false;

        public string? Notes { get; set; }
    }
}
