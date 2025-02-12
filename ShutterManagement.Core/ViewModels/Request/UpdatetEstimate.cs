using CsvHelper.Configuration.Attributes;
using ShutterManagement.Core.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdatetEstimate : CreateEstimate
    {
        [DefaultValue("")]
        [Required]
        public Guid EstimateId { get; set; }

        public bool? IsTaxApplied { get; set; } = false;

    }
}
