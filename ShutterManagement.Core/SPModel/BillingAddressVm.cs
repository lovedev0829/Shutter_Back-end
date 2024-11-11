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
        [RegularExpression("^[a-zA-Z]+$\r\n")]
        public string Label { get; set; }

        [Required]
        [MaxLength(255)]
        public string AddressLine1 { get; set; }

        [Required]
        [MaxLength(255)]
        public string AddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$\r\n")]

        public string City { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$\r\n")]

        public string  State {  get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression("^(0-9)")]

        public string ZipCode { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$\r\n")]
        public string Country { get; set; }
    }
}
