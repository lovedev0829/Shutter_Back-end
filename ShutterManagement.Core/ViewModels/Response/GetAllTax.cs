using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
    public class GetAllTax
    {
        public Guid?  TaxRateId { get; set; }
        //public Guid? Region { get; set; }
        //public string? RegionName { get; set; }

        //public decimal? Rate { get; set; }

        //public DateTime? EffectiveDate { get; set; }

        //public DateTime? ExpiryDate { get; set; }

        public string? TaxName { get; set; }

        [DefaultValue("")]
        [Required]
        public decimal? Municipality { get; set; }

        [DefaultValue("")]
        [Required]
        public decimal? State { get; set; }

        [DefaultValue("")]
        [Required]
        public decimal? City { get; set; }

        public bool? IsActive { get; set; }
        public string? TenantId { get; set; }
    }
}
