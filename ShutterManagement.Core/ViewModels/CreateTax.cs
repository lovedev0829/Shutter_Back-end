using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class CreateTax
    {
        [DefaultValue("")]
        [Required]
        public Guid? Region { get; set; }

        [DefaultValue("")]
        [Required]
        public decimal? Rate { get; set; }

        [DefaultValue("")]
        [Required]
        public DateTime? EffectiveDate { get; set; }

        [DefaultValue("")]
        [Required]
        public DateTime? ExpiryDate { get; set; }

    }
}
