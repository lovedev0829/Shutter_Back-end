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
        [MaxLength(255)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? MaterialName { get; set; }

        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        public decimal? UnitCost { get; set; }

        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        public string? UnitOfMeasurement { get; set; }

        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        public int? StockQuantity { get; set; }

        [DefaultValue("")]
        [Required]
        [MaxLength(255)]
        public int? ReorderLevel { get; set; }

        [DefaultValue("")]
        [Required]
      
        public Guid? VendorId { get; set; }

        public string? Notes { get; set; }
    }
}
