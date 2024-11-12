using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
     public class CreateAddress
    {

        public Guid? ContactId { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]

        public string? Label { get; set; }

        [Required]
        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string? City { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string? State { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression(@"^\d{10}$")]
        public string? ZipCode { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$")]
        public string? Country { get; set; }

        public Guid TenantId { get; set; }
    }
}
