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
        public string? TaxName { get; set; }

        [DefaultValue("")]
        public decimal? Municipality { get; set; }

        [DefaultValue("")]
        public decimal? State { get; set; }

        [DefaultValue("")]
        public decimal? City { get; set; }

        //[DefaultValue("")]
        //[Required]
        //public DateTime? EffectiveDate { get; set; }

        //[DefaultValue("")]
        //[Required]
        //public DateTime? ExpiryDate { get; set; }

    }
}
