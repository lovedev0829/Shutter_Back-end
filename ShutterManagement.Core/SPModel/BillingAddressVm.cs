using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel
{
     public class BillingAddressVm
    {
        public Guid BillingAddessId { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string Label { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string AddressLine1 { get; set; } = string.Empty ;

        [Required]
        [MaxLength(255)]
        public string AddressLine2 { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]

        public string City { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]

        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [RegularExpression("^(0-9)")]

        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string Country { get; set; } = string.Empty;
    }
}
