using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
     public class CreateAddress
    {

        public string? ContactId { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
       
        public string? Label { get; set; }

        [Required]
        [MaxLength(200)]
        [DefaultValue("")]
       
        public string? AddressLine1 { get; set; }

        [DefaultValue("")]
        [MaxLength(200)]

        public string? AddressLine2 { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
      
        public string? City { get; set; }

        [Required]
        [MaxLength(50)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
      
        public string? State { get; set; }

        [Required]
        [MaxLength(6)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
     
        public string? ZipCode { get; set; }

        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        [DefaultValue("")]
      
        public string? Country { get; set; }

        public Guid TenantId { get; set; }
    }
}
