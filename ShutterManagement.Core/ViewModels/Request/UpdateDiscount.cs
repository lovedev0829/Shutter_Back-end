using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateDiscount : CreateDiscount
    {
        [Required]
        [DefaultValue("")]
        public Guid? DiscountId { get; set; }
    }
}
